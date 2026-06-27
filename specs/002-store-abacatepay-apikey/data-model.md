# Phase 1 — Data Model: API_KEY do AbacatePay por Loja

## Entidade afetada: `Store`

A feature adiciona **um** atributo à entidade existente `Store`. Nenhuma entidade nova.

### Atributo novo

| Atributo | Tipo (.NET) | Coluna (PostgreSQL) | Nulo? | Tamanho | Sensível | Observações |
|----------|-------------|---------------------|-------|---------|----------|-------------|
| `AbacatePayApiKey` | `string` | `abacatepay_api_key` | Sim | `varchar(500)` | **Sim (secret)** | Coluna e entrada no `ModelSnapshot` **já existem**; write-only pela API |

### Camadas e onde o atributo aparece

| Camada | Tipo | Inclui `AbacatePayApiKey`? | Justificativa |
|--------|------|---------------------------|---------------|
| Entidade Infra | `ProxyPay.Infra.Context.Store` | **Sim** (novo) | Persistência |
| Mapeamento EF | `ProxyPayContext.OnModelCreating` | **Sim** (novo, Fluent) | Coluna `abacatepay_api_key`, `HasMaxLength(500)` |
| Domain Model | `ProxyPay.Domain.Models.StoreModel` | **Sim** (novo) | Lógica de negócio (`SetAbacatePayApiKey`) |
| DTO de leitura | `ProxyPay.DTO.Store.StoreInfo` | **Não** | Write-only — nunca retornado (FR-005) |
| DTO insert | `StoreInsertInfo` | **Não** | Criação não define credencial (FR-007) |
| DTO update geral | `StoreUpdateInfo` | **Não** | Update geral não toca a credencial (FR-007) |
| DTO dedicado | `StoreApiKeyUpdateInfo` (novo) | **Sim** (`apiKey`) | Único caminho de escrita |
| GraphQL `Store` | `StoreType` | **Não** (oculto) + `hasAbacatePayApiKey` | Field hiding + indicador (FR-006, FR-013) |

## DTO novo: `StoreApiKeyUpdateInfo`

```csharp
namespace ProxyPay.DTO.Store;

public class StoreApiKeyUpdateInfo
{
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; }
}
```

> `storeId` vem da rota (`/store/{storeId}/abacatepay-apikey`), não do corpo, seguindo o estilo do `DELETE /store/{storeId}` existente.

## Regras de validação

| Regra | Onde | Erro |
|-------|------|------|
| `apiKey` obrigatória / não vazia (após trim) | `StoreApiKeyUpdateInfoValidator` (FluentValidation) + guarda em `StoreModel.SetAbacatePayApiKey` | `BadRequest` com mensagem clara (FR-008) |
| Loja deve existir | `StoreService.UpdateAbacatePayApiKeyAsync` (`GetByIdAsync`) | `BadRequest` "Store not found" (FR-009) |
| Solicitante deve ser dono da loja | `StoreModel.ValidateOwnership(userId)` | `Forbid` (403) — `UnauthorizedAccessException` (FR-003) |
| Normalização: `Trim()` antes de armazenar | `StoreModel.SetAbacatePayApiKey` | — (Edge Case) |
| Sem validação on-line contra AbacatePay | — | N/A (FR-016) |

## Comportamento (estados) da credencial

```
[ausente/null] --(PUT apiKey não-vazia)--> [configurada]
[configurada]  --(PUT apiKey não-vazia)--> [configurada]   (substitui valor; FR-004)
[configurada]  --(PUT apiKey vazia)------> [configurada]   (rejeitado; valor preservado; FR-008)
[ausente/null] --(operação de pagamento)-> ERRO claro      (sem fallback; FR-014)
[configurada]  --(operação de pagamento)-> usa a credencial da loja (FR-010)
```

> Remoção/limpeza da credencial está **fora de escopo** (Assumptions da spec).

## Métodos de domínio novos

```csharp
// StoreModel
public void SetAbacatePayApiKey(string apiKey)
{
    if (string.IsNullOrWhiteSpace(apiKey))
        throw new ArgumentException("AbacatePay API key cannot be empty.", nameof(apiKey));
    AbacatePayApiKey = apiKey.Trim();
    MarkUpdated();
}

// IStoreService
Task UpdateAbacatePayApiKeyAsync(long storeId, string apiKey, long userId);
```

## Isolamento por tenant

Sem alteração estrutural: cada tenant tem seu próprio banco; `proxypay_stores` (e portanto a credencial) reside no banco do tenant resolvido por `X-Tenant-Id`. Nenhuma coluna `tenant_id` é introduzida (FR-011 garantido pelo isolamento físico existente).
