# Feature Specification: Testes de API para todos os endpoints existentes

**Feature Branch**: `003-api-endpoint-tests`  
**Created**: 2026-06-26  
**Status**: Draft  
**Input**: User description: "Crie um teste de API para todos os endpoints existentes, use a skill dotnet-test-api"

## User Scenarios & Testing *(mandatory)*

A suíte de testes de API valida, de ponta a ponta e via HTTP real, o comportamento de **todos os endpoints expostos** pela API ProxyPay. Cada endpoint é exercitado contra uma instância em execução, autenticando-se uma única vez por sessão de testes e enviando o cabeçalho de tenant exigido pela arquitetura multi-tenant. O objetivo é detectar regressões de contrato (status HTTP, formato de resposta, regras de autorização e validação) antes que cheguem a produção.

Os endpoints atualmente existentes, agrupados por área, são:

- **Store** (requer autenticação): criar loja, atualizar loja, definir a API key da AbacatePay, excluir loja.
- **Payment** (anônimo): criar cobrança recorrente (billing), criar fatura (invoice), gerar QR Code PIX, consultar status do QR Code, simular pagamento.
- **Webhook** (anônimo, protegido por segredo): receber notificações da AbacatePay.
- **GraphQL** (requer autenticação): consultas `myStores`, `myInvoices`, `myInvoiceByNumber`, `myTransactions`, `myBalance`, `myCustomers`.
- **GraphQL docs** (requer autenticação): endpoint de documentação/stub para Swagger.

### User Story 1 - Validar os endpoints de pagamento (Priority: P1)

Como responsável pela qualidade da API, quero verificar automaticamente que os endpoints de pagamento (billing, invoice, qrcode, status do qrcode e simulação de pagamento) respondem corretamente a entradas válidas e inválidas, garantindo que o fluxo central de cobrança PIX permaneça íntegro a cada alteração.

**Why this priority**: Os endpoints de pagamento representam o valor central do produto (orquestração de cobranças PIX). Uma regressão aqui impacta diretamente a receita dos lojistas e é o caminho mais crítico do sistema.

**Independent Test**: Pode ser totalmente testado executando a API, enviando requisições aos cinco endpoints de pagamento com payloads válidos e inválidos e confirmando os status HTTP e o formato das respostas — sem depender dos demais grupos de endpoints.

**Acceptance Scenarios**:

1. **Given** uma loja válida identificada por seu ClientId, **When** uma requisição de criação de cobrança é enviada com cliente e dados válidos, **Then** a resposta retorna sucesso com os dados da cobrança criada.
2. **Given** uma requisição de criação de cobrança sem dados do cliente, **When** a requisição é enviada, **Then** a resposta indica erro de validação informando que o cliente é obrigatório.
3. **Given** uma requisição de criação de cobrança com cliente sem e-mail, **When** a requisição é enviada, **Then** a resposta indica erro de validação informando que o e-mail é obrigatório.
4. **Given** uma fatura existente identificada por seu Id, **When** o status do QR Code é consultado, **Then** a resposta retorna o status atual da fatura.
5. **Given** uma fatura existente, **When** a simulação de pagamento é solicitada, **Then** a resposta confirma a simulação realizada.

---

### User Story 2 - Validar os endpoints de loja e suas regras de autorização (Priority: P2)

Como responsável pela qualidade da API, quero verificar que os endpoints de gestão de loja (criar, atualizar, definir API key da AbacatePay e excluir) exigem autenticação, respeitam a propriedade do recurso e retornam os status corretos, evitando que usuários acessem ou alterem lojas de terceiros.

**Why this priority**: A gestão de lojas controla o acesso aos recursos financeiros de cada lojista. Falhas de autorização aqui são incidentes de segurança, mas o fluxo é secundário ao processamento de pagamentos em si.

**Independent Test**: Pode ser testado autenticando-se na sessão, criando uma loja, atualizando-a, definindo sua API key e excluindo-a, além de confirmar que requisições sem credenciais são rejeitadas — independente dos endpoints de pagamento.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado, **When** envia uma requisição válida de criação de loja, **Then** a resposta confirma a criação e retorna o identificador da loja.
2. **Given** uma requisição de qualquer endpoint de loja sem credenciais de autenticação, **When** a requisição é enviada, **Then** a resposta indica acesso não autorizado.
3. **Given** um usuário autenticado que não é dono de uma loja, **When** tenta atualizá-la, definir sua API key ou excluí-la, **Then** a resposta indica acesso proibido.
4. **Given** um usuário autenticado dono de uma loja, **When** define a API key da AbacatePay com valor válido, **Then** a resposta confirma a operação sem conteúdo.

---

### User Story 3 - Validar o endpoint de webhook e suas proteções (Priority: P3)

Como responsável pela qualidade da API, quero verificar que o endpoint de webhook da AbacatePay aceita notificações apenas com o segredo correto e responde de forma consistente a payloads inválidos, garantindo que eventos externos sejam processados com segurança.

**Why this priority**: O webhook mantém o status das faturas sincronizado com o provedor de pagamentos, mas é acionado de forma assíncrona e tolera reprocessamento, tornando-o menos crítico que os fluxos síncronos diretos.

**Independent Test**: Pode ser testado enviando notificações ao endpoint de webhook com e sem o segredo correto, e com payloads completos e incompletos, confirmando o comportamento de resposta esperado.

**Acceptance Scenarios**:

1. **Given** uma notificação de webhook sem o segredo correto, **When** é enviada, **Then** a requisição é aceita silenciosamente sem processar o evento.
2. **Given** uma notificação de webhook com o segredo correto mas sem dados de evento, **When** é enviada, **Then** a requisição é aceita sem processar o evento.
3. **Given** uma notificação de webhook com segredo correto e payload completo, **When** é enviada, **Then** a requisição é aceita e o evento é processado.

---

### User Story 4 - Validar as consultas GraphQL autenticadas (Priority: P3)

Como responsável pela qualidade da API, quero verificar que as consultas GraphQL autenticadas (`myStores`, `myInvoices`, `myInvoiceByNumber`, `myTransactions`, `myBalance`, `myCustomers`) exigem autenticação e retornam dados apenas do usuário autenticado, garantindo o isolamento de dados entre lojistas.

**Why this priority**: As consultas GraphQL são de leitura e alimentam painéis administrativos; uma falha degrada a visualização, mas não interrompe transações financeiras.

**Independent Test**: Pode ser testado enviando consultas GraphQL ao endpoint com e sem autenticação e confirmando que dados são retornados apenas para sessões autenticadas.

**Acceptance Scenarios**:

1. **Given** uma sessão autenticada, **When** a consulta `myStores` é executada, **Then** a resposta retorna as lojas do usuário autenticado.
2. **Given** uma requisição GraphQL sem credenciais, **When** uma consulta autenticada é executada, **Then** a resposta indica acesso não autorizado ou ausência de dados.
3. **Given** uma sessão autenticada, **When** cada uma das consultas autenticadas é executada, **Then** todas retornam uma resposta estruturalmente válida sem erros de execução.

---

### Edge Cases

- O que acontece quando um identificador de fatura inexistente é usado na consulta de status ou na simulação de pagamento? A resposta deve indicar erro de forma controlada, sem expor detalhes internos.
- Como o sistema se comporta quando o cabeçalho de tenant exigido está ausente ou inválido em uma requisição? A suíte deve cobrir esse cenário garantindo rejeição ou comportamento previsível.
- O que acontece quando um payload malformado (JSON inválido ou campos obrigatórios ausentes) é enviado a qualquer endpoint? A resposta deve ser um erro de validação, nunca uma falha não tratada.
- Como o webhook se comporta quando o mesmo evento é entregue mais de uma vez (idempotência)? O reprocessamento não deve corromper o estado da fatura — alinhado à premissa de testes idempotentes que toleram reexecução.
- O que acontece quando um token de autenticação expirado é usado nos endpoints autenticados? A resposta deve indicar não autorizado.

## Clarifications

### Session 2026-06-26

- Q: Como a suíte deve lidar com a dependência externa da AbacatePay? → A: API configurada contra o modo sandbox/DevMode da AbacatePay (respostas reais e determinísticas do sandbox; assertions de contrato sobre elas).
- Q: Como garantir execução repetível sem dados residuais (FR-013)? → A: Sem limpeza explícita; os testes são idempotentes e toleram dados residuais entre execuções.
- Q: Como a instância da API sob teste deve ser provida? → A: A suíte assume uma API já em execução numa BaseUrl configurável e testa via HTTP externo (padrão da skill `dotnet-test-api`).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A suíte de testes MUST cobrir todos os endpoints HTTP existentes da API, agrupados por área (Store, Payment, Webhook, GraphQL e GraphQL docs), sem deixar nenhum endpoint sem ao menos um teste.
- **FR-002**: A suíte de testes MUST exercitar cada endpoint contra uma instância da API em execução, comunicando-se exclusivamente via HTTP (testes de ponta a ponta externos).
- **FR-003**: A suíte de testes MUST autenticar-se uma única vez por sessão de testes e reutilizar a credencial obtida em todos os testes que exigem autenticação.
- **FR-004**: A suíte de testes MUST enviar o cabeçalho de identificação de tenant exigido pela arquitetura multi-tenant em todas as requisições.
- **FR-005**: Para cada endpoint, a suíte MUST validar pelo menos um cenário de sucesso (entrada válida → status esperado e resposta no formato esperado).
- **FR-006**: Para cada endpoint que aplica validação de entrada, a suíte MUST validar pelo menos um cenário de falha de validação (entrada inválida → erro esperado).
- **FR-007**: Para cada endpoint autenticado, a suíte MUST validar que requisições sem credenciais são rejeitadas como não autorizadas.
- **FR-008**: Para os endpoints de loja que verificam propriedade do recurso, a suíte MUST validar que um usuário sem permissão recebe resposta de acesso proibido.
- **FR-009**: A suíte MUST validar que o endpoint de webhook ignora notificações sem o segredo correto e processa notificações com segredo e payload válidos.
- **FR-010**: A suíte MUST validar que as consultas GraphQL autenticadas retornam dados apenas para sessões autenticadas.
- **FR-011**: A suíte de testes MUST residir em um projeto de testes dedicado e separado do projeto de testes unitários, conforme o padrão de testes de API do projeto.
- **FR-012**: A suíte MUST produzir um resultado claro de aprovação/reprovação por teste, permitindo identificar exatamente qual endpoint e cenário falhou.
- **FR-013**: A suíte MUST ser idempotente e tolerar dados residuais entre execuções — não depende de uma etapa de limpeza para que execuções repetidas produzam o mesmo resultado (sem assumir banco/tenant limpo a cada rodada).
- **FR-014**: A suíte MUST exercitar a API configurada contra o modo sandbox/DevMode da AbacatePay; as assertions sobre respostas dos endpoints de pagamento e webhook validam o contrato retornado pelo sandbox, sem acionar o provedor real de produção.
- **FR-015**: A suíte MUST assumir uma instância da API já em execução, alcançável por uma BaseUrl fornecida via configuração, e comunicar-se com ela exclusivamente via HTTP externo (a suíte não sobe a API em processo).

### Key Entities *(include if feature involves data)*

- **Suíte de Testes de API**: Conjunto organizado de casos de teste, um ou mais por endpoint, agrupados por área funcional. Atributos relevantes: endpoint alvo, cenário (sucesso/falha/autorização), resultado esperado.
- **Sessão de Testes Autenticada**: Contexto compartilhado que mantém a credencial de autenticação e o identificador de tenant válidos por toda a execução da suíte.
- **Loja (Store)**: Recurso de teste criado/manipulado pelos testes de loja; identificado por StoreId e ClientId; associada a um dono.
- **Cobrança/Fatura/QR Code (Payment)**: Recursos financeiros criados pelos testes de pagamento; identificados por Id; vinculados a uma loja e a um cliente.
- **Notificação de Webhook**: Evento externo simulado pelos testes de webhook; contém tipo de evento, identificador de dado e segredo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos endpoints HTTP existentes têm ao menos um teste de cenário de sucesso na suíte.
- **SC-002**: 100% dos endpoints autenticados têm um teste que confirma a rejeição de requisições não autenticadas.
- **SC-003**: 100% dos endpoints com validação de entrada têm ao menos um teste de cenário de falha de validação.
- **SC-004**: A suíte completa pode ser executada por um desenvolvedor com um único comando e conclui sem intervenção manual.
- **SC-005**: Quando um endpoint apresenta uma regressão de contrato, ao menos um teste da suíte falha, identificando inequivocamente o endpoint e o cenário afetados.
- **SC-006**: A suíte pode ser executada repetidamente (pelo menos duas execuções consecutivas) com resultados idênticos, sem falhas causadas por dados residuais.

## Assumptions

- Existe um ambiente de testes (loja, tenant e usuário de teste) com credenciais válidas disponíveis para a sessão de testes autenticar-se uma vez por execução; segredos e URLs são fornecidos via configuração, não embutidos no código.
- A autenticação utiliza o esquema Bearer/JWT do projeto (NAuth), consistente com os controllers que usam `[Authorize]` e `IUserClient`.
- O conjunto de endpoints a cobrir é exatamente o atualmente exposto pela API (Store, Payment, Webhook, GraphQL e GraphQL docs); novos endpoints adicionados no futuro estão fora do escopo desta entrega inicial.
- O endpoint GraphQL único (`/graphql`) atende às consultas autenticadas listadas; a cobertura GraphQL valida o contrato das consultas, não cada campo individual de cada tipo.
- A AbacatePay é acionada em modo sandbox/DevMode no ambiente de teste; as respostas do sandbox são suficientemente estáveis para assertions de contrato, e nenhuma transação real de produção é criada (ver FR-014).
- A suíte assume que a API já está em execução numa BaseUrl configurável (a suíte não inicia a API); a BaseUrl, credenciais e segredos são fornecidos via configuração, não embutidos no código (ver FR-015).
- A suíte é implementada com o stack de testes de API do projeto (xUnit + Flurl.Http + FluentAssertions com fixture compartilhada `IAsyncLifetime`), conforme a skill `dotnet-test-api`.
- Os testes são idempotentes e não realizam limpeza de dados ao final; toleram recursos remanescentes de execuções anteriores (ver FR-013).
- O ambiente alvo dos testes é uma instância local/de desenvolvimento da API, não produção.
