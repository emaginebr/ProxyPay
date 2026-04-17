# Feature Specification: Multi-Tenant com tenant inicial "fortuno"

**Feature Branch**: `001-multi-tenant-fortuno`
**Created**: 2026-04-17
**Status**: Draft
**Input**: User description: "Preciso que o ProxyPay funcione em multi-tenant. - Use a skill dotnet-multi-tenant - Pode usar de base o projeto c:\\repos\\Lofn\\Lofn - O novo tenant se chamará fortuno"

## Clarifications

### Session 2026-04-17

- Q: Estratégia para a base operacional atual do ProxyPay na estreia da
  multi-tenancy → A: **(Revisto 2026-04-17 após investigação do código —
  ver nota abaixo)** A multi-tenancy já estava parcialmente implementada
  no ProxyPay: os tenants **"emagine"** (DB dedicado `emagine` em
  produção DigitalOcean) e **"monexup"** (DB dedicado `monexup` no mesmo
  cluster DO) já existem como tenants ativos. Esta feature adiciona
  **"fortuno"** como **terceiro** tenant, inicializado vazio, sem alterar
  os dois já existentes. Portanto, ao término, a plataforma opera com
  **três** tenants: emagine (legado), monexup (legado) e fortuno (novo).
- Q: Como o tenant é resolvido em tráfego público/anônimo (GraphQL
  público, vitrines `/@/:sellerSlug`, rotas `/:networkSlug`) que hoje não
  exige autenticação? → A: Cabeçalho HTTP de tenant é obrigatório em
  **toda** requisição, autenticada ou anônima. Endpoints públicos que
  chegarem sem o cabeçalho são rejeitados explicitamente. O frontend e
  todos os clientes externos que consomem endpoints públicos DEVEM passar
  a enviar o cabeçalho.
- Q: Ciclo de vida do tenant — desativação/arquivamento/exclusão → A:
  **Fora de escopo na v1.** Nesta entrega tenants apenas são criados; não
  há funcionalidade de desativar, arquivar ou excluir tenant.
  Desativação/exclusão permanecem para iteração futura quando surgir caso
  operacional concreto.
- Q: Formato/validação do identificador do tenant (pattern, tamanho,
  case) → A: **Não é necessária validação** de formato do identificador
  nesta entrega. A plataforma trata o valor recebido no cabeçalho como
  string opaca; a consulta ao catálogo de tenants (FR-004) serve como
  filtro implícito — identificadores desconhecidos são simplesmente
  rejeitados por "não resolvíveis".

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Isolamento total de dados entre tenants (Priority: P1)

Como operador da plataforma, preciso que cada tenant tenha seus próprios dados
(lojas, produtos, pedidos, usuários, imagens) completamente isolados dos demais,
de modo que um usuário autenticado no tenant "fortuno" jamais veja, edite ou
receba em respostas da API quaisquer registros pertencentes a outro tenant.

**Why this priority**: Sem isolamento de dados não existe multi-tenancy real —
este é o requisito fundacional de segurança e compliance. Qualquer vazamento
cross-tenant invalida todo o produto.

**Independent Test**: Pode ser totalmente validado com os dois tenants reais
("emagine" e "fortuno") povoados com datasets conhecidos, executando as
mesmas consultas autenticadas em cada um e confirmando que nenhum registro
de um tenant aparece nas respostas do outro — tanto em leitura quanto em
tentativas de modificação direta por ID.

**Acceptance Scenarios**:

1. **Given** os tenants "emagine" (com a base migrada) e "fortuno" (novo, com
   lojas próprias cadastradas), **When** um usuário autenticado em "fortuno"
   solicita a listagem das suas lojas, **Then** a resposta contém
   exclusivamente as lojas de "fortuno" e nenhuma de "emagine".
2. **Given** uma loja pertencente a "emagine" com ID conhecido, **When** um
   usuário autenticado em "fortuno" tenta acessá-la ou modificá-la por ID,
   **Then** a plataforma responde como se o recurso não existisse para aquele
   tenant (sem vazar metadados).
3. **Given** o mesmo identificador numérico de pedido existindo em "emagine"
   e em "fortuno", **When** consultado sob o contexto de "fortuno", **Then**
   somente o pedido de "fortuno" é retornado.

---

### User Story 2 - Roteamento automático por tenant em toda requisição (Priority: P1)

Como desenvolvedor consumindo a API, preciso que toda requisição autenticada
seja automaticamente associada ao tenant correto sem que o cliente precise
repetir filtros por tenant em cada chamada, e sem que o desenvolvedor
backend precise lembrar de aplicar esse filtro em cada consulta.

**Why this priority**: A maioria das falhas de isolamento em sistemas
multi-tenant ocorre por esquecimento humano em aplicar o filtro. Tornar o
roteamento automático é o que torna o isolamento do US1 robusto na prática,
e por isso está em P1 junto com US1.

**Independent Test**: Pode ser validado enviando requisições idênticas com
credenciais de tenants diferentes para o mesmo endpoint e verificando que as
respostas refletem apenas o escopo do tenant correspondente — sem que nenhum
parâmetro explícito de tenant tenha sido passado no payload ou na query.

**Acceptance Scenarios**:

1. **Given** um endpoint que lista produtos, **When** a mesma requisição é
   enviada com credenciais do tenant "fortuno" e depois com credenciais de
   outro tenant, **Then** cada resposta contém apenas os produtos do tenant
   correspondente, sem necessidade de parâmetro adicional no request.
2. **Given** um endpoint de criação de pedido, **When** um usuário
   autenticado em "fortuno" cria um pedido, **Then** o pedido é persistido
   no escopo de "fortuno" automaticamente, sem que o cliente envie o tenant
   no payload.
3. **Given** uma requisição sem indicação válida de tenant, **When** chega à
   API, **Then** ela é rejeitada com erro claro de contexto de tenant
   ausente, antes de qualquer acesso a dados.

---

### User Story 3 - Credenciais de autenticação isoladas por tenant (Priority: P1)

Como responsável de segurança, preciso que os segredos utilizados para emitir
e validar tokens de autenticação sejam distintos por tenant, de modo que um
token emitido para o tenant "fortuno" não seja aceito como válido em outro
tenant, mesmo que o mesmo identificador de usuário exista em ambos.

**Why this priority**: Sem isolamento dos segredos de autenticação, um token
roubado ou reutilizado de um tenant poderia ser usado para acessar outro
tenant, neutralizando o isolamento de dados. Por isso também é P1.

**Independent Test**: Pode ser validado emitindo um token no tenant "fortuno"
e tentando usá-lo explicitamente como credencial em uma requisição destinada
a outro tenant; a requisição deve ser rejeitada como não autenticada.

**Acceptance Scenarios**:

1. **Given** um token emitido sob o segredo do tenant "fortuno", **When**
   esse token é apresentado em uma requisição endereçada a outro tenant,
   **Then** a requisição é rejeitada como não autenticada.
2. **Given** o mesmo identificador de usuário existente em dois tenants,
   **When** cada tenant emite seu token, **Then** cada token só é aceito no
   tenant que o emitiu.
3. **Given** a rotação do segredo de um tenant, **When** um token emitido
   antes da rotação é apresentado, **Then** é rejeitado sem afetar tokens de
   outros tenants.

---

### User Story 4 - Provisionamento de "fortuno" alongside "emagine" e "monexup" (Priority: P2)

Como operador da plataforma, preciso colocar **"fortuno"** em operação
como terceiro tenant da plataforma, preservando o funcionamento dos dois
tenants legados já em produção (**"emagine"** com DB `emagine` e
**"monexup"** com DB `monexup`). Ao término desta entrega os três tenants
coexistem com bancos dedicados e segredos de autenticação independentes.

**Why this priority**: É o marco de entrega concreto desta feature — sem os
dois tenants operacionais a funcionalidade não gera valor de negócio, mas
depende do isolamento correto (US1–US3) para ser seguro; por isso P2.

**Independent Test**: Pode ser validado executando o plano de migração que
transforma a base atual em "emagine", provisionando "fortuno" como banco
limpo, autenticando em cada um deles e executando operações fim-a-fim
(login → listar lojas → criar produto → criar pedido) nos dois tenants,
confirmando que "emagine" mantém os registros pré-existentes e "fortuno"
parte do zero.

**Acceptance Scenarios**:

1. **Given** a base operacional atual do ProxyPay, **When** o operador
   executa o procedimento de migração, **Then** essa base passa a ser o
   tenant "emagine" com banco dedicado e segredo de autenticação próprio
   registrados no catálogo de tenants.
2. **Given** "emagine" migrado, **When** o operador executa o procedimento
   de provisionamento de "fortuno", **Then** "fortuno" fica disponível com
   banco dedicado vazio e segredo de autenticação próprio.
3. **Given** "emagine" e "fortuno" provisionados, **When** um usuário
   autenticado em "emagine" executa o fluxo completo (login → listar lojas
   → criar produto → criar pedido), **Then** todas as operações são
   persistidas no banco dedicado a "emagine" e nenhuma flui para o banco de
   "fortuno" (e vice-versa quando o usuário é de "fortuno").
4. **Given** "emagine" e "fortuno" em operação, **When** um terceiro tenant
   é adicionado futuramente, **Then** o procedimento de provisionamento é o
   mesmo usado para "fortuno", sem exigir alteração de código do produto.

---

### Edge Cases

- **Requisição sem contexto de tenant**: toda chamada — autenticada ou
  anônima, incluindo endpoints públicos (GraphQL público, vitrines) — que
  não consiga determinar o tenant de origem deve ser rejeitada
  explicitamente, nunca cair silenciosamente em um tenant "default".
- **Consumidores externos de endpoints públicos**: clientes que hoje
  consomem endpoints anônimos sem cabeçalho (crawlers, integrações de
  parceiros, deep links) passam a falhar até serem atualizados para
  enviar o cabeçalho de tenant — este é um efeito esperado desta entrega.
- **Tenant desconhecido**: requisição que aponte para um tenant inexistente
  deve falhar com erro claro, sem vazar a lista de tenants válidos.
- **Tenant temporariamente indisponível** (ex.: manutenção do banco do
  tenant): deve retornar erro operacional sem afetar outros tenants.
- **Reúso de IDs entre tenants**: IDs numéricos podem colidir entre tenants
  (cada tenant tem seu próprio espaço de IDs); o sistema não pode confundir
  um recurso de um tenant com o de outro apenas pelo ID.
- **Token expirado vs. token de outro tenant**: as respostas para ambos os
  casos devem ser indistinguíveis externamente para evitar enumeração de
  tenants por oráculo de erro.
- **Chamadas internas entre serviços** (API → BackgroundService, API → ACLs
  externas): o contexto do tenant deve ser propagado automaticamente para
  que operações assíncronas e chamadas externas continuem no tenant correto.
- **Migrações de esquema**: ao evoluir o schema, a migração deve ser
  aplicada a todos os bancos de tenants de forma consistente e auditável.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A plataforma DEVE suportar múltiplos tenants logicamente
  independentes, cada um com seu próprio conjunto de dados (lojas, produtos,
  pedidos, imagens, usuários de loja, etc.).
- **FR-002**: Cada tenant DEVE ter seu próprio banco de dados relacional
  dedicado, isolado dos demais tenants em nível de armazenamento.
- **FR-003**: A plataforma DEVE identificar o tenant de origem de cada
  requisição HTTP antes de qualquer operação de dados.
- **FR-004**: Toda requisição HTTP — autenticada ou anônima, incluindo as
  que chegam aos endpoints públicos (GraphQL público, vitrines públicas,
  rotas por slug de network/seller) — sem um contexto de tenant válido e
  resolvível DEVE ser rejeitada com erro explícito, sem acessar dados.
- **FR-005**: Toda operação de leitura ou escrita DEVE ser confinada
  automaticamente ao tenant da requisição, sem necessidade de o
  desenvolvedor aplicar filtro manual em cada consulta.
- **FR-006**: Cada tenant DEVE possuir seu próprio segredo para emissão e
  validação dos tokens de autenticação; segredos NÃO DEVEM ser
  compartilhados entre tenants.
- **FR-007**: Tokens emitidos para um tenant NÃO DEVEM ser aceitos como
  autenticação em outro tenant, mesmo que o identificador de usuário exista
  em ambos.
- **FR-008**: O contexto do tenant corrente DEVE ser propagado
  automaticamente para chamadas a serviços externos (ACL) realizadas no
  atendimento da requisição.
- **FR-009**: O contexto do tenant DEVE ser preservado em operações
  assíncronas iniciadas a partir de uma requisição (ex.: trabalhos de
  background disparados por uma ação do usuário).
- **FR-010**: A plataforma DEVE permitir o provisionamento de um novo
  tenant (banco dedicado, segredo dedicado, configuração inicial) sem
  necessidade de alterar o código da aplicação.
- **FR-011**: Ao final desta feature, **três** tenants DEVEM estar
  provisionados e operacionais em produção, cada um com banco dedicado e
  segredo de autenticação próprio: **"emagine"** (pré-existente, DB
  `emagine`), **"monexup"** (pré-existente, DB `monexup`) e **"fortuno"**
  (novo, DB `fortuno` a ser criado vazio). Os dois primeiros continuam
  intocados; a entrega adiciona fortuno sem interromper os existentes.
- **FR-012**: Erros de autenticação por token inválido e por token de outro
  tenant DEVEM retornar respostas externamente indistinguíveis para evitar
  enumeração de tenants.
- **FR-013**: Tentativas de acessar por ID um recurso pertencente a outro
  tenant DEVEM se comportar como recurso inexistente, sem vazar metadados
  do recurso alheio.
- **FR-013a**: Os endpoints públicos do ProxyPay (GraphQL público,
  vitrines públicas de loja/seller/network) DEVEM obrigar o cabeçalho de
  tenant na mesma regra dos endpoints autenticados — ausência ou valor
  inválido resulta em rejeição antes de qualquer acesso a dados.
- **FR-014**: A plataforma DEVE permitir rotação do segredo de autenticação
  de um tenant específico sem impactar tokens ou disponibilidade de outros
  tenants.
- **FR-015**: Migrações de esquema DEVEM ser aplicáveis de forma consistente
  a todos os bancos de tenants, com registro auditável de quais tenants
  receberam cada migração.
- **FR-016**: A estreia desta feature NÃO introduz migração de dados de
  negócio — os tenants "emagine" e "monexup" já possuem bancos dedicados
  em produção (`emagine` e `monexup` no cluster DO) e já estão
  operacionais. A única ação de provisionamento desta feature é **criar
  o banco dedicado "fortuno"** no mesmo cluster e inicializá-lo vazio.
  Após a estreia, nenhum dado de negócio permanece fora do escopo de um
  tenant, e os três tenants coexistem com isolamento físico pleno.

### Key Entities *(include if feature involves data)*

- **Tenant**: unidade de isolamento lógico. Identificado por um nome
  estável (ex.: "emagine", "fortuno"), possui conexão de banco própria,
  segredo de autenticação próprio e configuração operacional própria. É
  referenciado implicitamente em toda requisição autenticada.
- **Contexto de Tenant da Requisição**: informação transiente, derivada da
  requisição, que indica sob qual tenant aquela execução está operando.
  Visível para toda a camada de domínio/persistência durante o atendimento.
- **Catálogo de Tenants**: registro das tenants ativas da plataforma,
  associando cada tenant à sua string de conexão, ao seu segredo de
  autenticação e ao seu status operacional. Usado para resolver o tenant a
  partir da requisição e carregar dinamicamente a configuração certa.
- **Dados de Negócio Escopados por Tenant**: lojas, produtos, categorias,
  pedidos, imagens e usuários de loja — todas as entidades de domínio do
  ProxyPay passam a existir sempre dentro do escopo de um tenant.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos endpoints que retornam ou modificam dados de
  negócio, auditoria independente confirma que nenhuma resposta inclui
  registros pertencentes a outro tenant — verificação feita com pelo menos
  dois tenants povoados e um conjunto sistemático de requisições cruzadas.
- **SC-002**: 100% das requisições recebidas sem contexto de tenant
  resolvível são rejeitadas antes de qualquer acesso a dados, com erro
  operacional claro.
- **SC-003**: Um token emitido para um tenant tem taxa de aceitação 0% em
  qualquer outro tenant em testes sistemáticos de reutilização de token.
- **SC-004**: Os tenants "emagine" e "fortuno" estão operacionais em
  produção e um usuário real de cada tenant consegue completar o fluxo
  fim-a-fim (login → listar lojas → criar produto → criar pedido) sem
  erros em até 2 minutos.
- **SC-005**: Adicionar um terceiro tenant após "emagine" e "fortuno"
  exige somente a execução do procedimento de provisionamento documentado,
  sem nenhuma alteração no código da aplicação.
- **SC-006**: Uma rotação do segredo de autenticação de um tenant afeta
  exclusivamente o tenant alvo — tokens e disponibilidade dos demais
  tenants permanecem intactos (verificação com teste controlado).
- **SC-007**: Aplicar uma migração de esquema nova resulta em 100% dos
  bancos de tenants ativos atualizados, com registro auditável por tenant.

## Assumptions

- O tenant é identificado por meio de um cabeçalho HTTP dedicado enviado
  pelo cliente em **toda** requisição (autenticada ou anônima), conforme o
  padrão adotado pela skill `dotnet-multi-tenant` (TenantHeaderHandler) e
  já praticado no projeto de referência em `c:\repos\Lofn\Lofn`. Caso a
  decisão estratégica futura seja outra (subdomínio, claim de token,
  prefixo de rota), o mecanismo de resolução é um ponto único de troca e
  não altera os requisitos funcionais desta especificação.
- O provisionamento de novos tenants (criação do banco, geração de
  segredo, registro no catálogo) é operado por infraestrutura / equipe de
  plataforma, não exposto como funcionalidade self-service ao usuário
  final nesta entrega — automatizar a criação via UI de administração é
  fora de escopo aqui.
- O frontend React do ProxyPay passará a enviar o cabeçalho de tenant em
  todas as requisições autenticadas. Como dois tenants ("emagine" e
  "fortuno") coexistirão desde o lançamento, a definição de qual tenant o
  cliente web usa em cada deployment (ex.: build/ambiente dedicado por
  tenant, domínio distinto por tenant, ou seleção no login) é tratada como
  concern do frontend e não altera os requisitos funcionais do backend
  definidos aqui; um fluxo self-service de escolha de tenant para o
  usuário final continua fora de escopo nesta entrega.
- A skill `dotnet-multi-tenant` e o projeto `c:\repos\Lofn\Lofn` servem
  como referência arquitetural (TenantMiddleware, TenantContext,
  TenantResolver, TenantHeaderHandler, TenantDbContextFactory,
  configuração dinâmica de JWT); a implementação desta feature reutilizará
  esses padrões em vez de inventar abordagens novas.
- Entidades já existentes no domínio do ProxyPay (stores, products,
  orders, images, storeusers, categories, etc.) passam a ser implicitamente
  escopadas por tenant via o banco de dados ativo da requisição, sem
  exigir colunas adicionais de `tenant_id` nas tabelas dentro de cada
  banco — isolamento é obtido por separação física de bancos.
- Não há requisito de consultas cross-tenant em tempo real para usuários
  finais nesta entrega; relatórios agregados entre tenants, se
  necessários, serão tratados em iteração posterior fora deste escopo.
- Desativação, arquivamento e exclusão de tenant estão fora de escopo
  nesta entrega. Uma vez provisionado, um tenant permanece ativo até
  iteração futura que introduza um ciclo de vida explícito.
- Docker/`docker compose` não será utilizado em ambiente de desenvolvimento
  local (conforme a constituição); validação local usará execução direta
  dos projetos .NET contra instâncias PostgreSQL acessíveis sem contêiner.

## Dependencies

- Projeto de referência em `c:\repos\Lofn\Lofn` permanece disponível e
  legível durante o planejamento e a implementação.
- Skill `dotnet-multi-tenant` disponível para uso na fase de planejamento e
  implementação.
- Infraestrutura capaz de provisionar um banco PostgreSQL dedicado ao
  tenant "fortuno" e de armazenar/rotacionar segredos por tenant fora do
  código-fonte.
