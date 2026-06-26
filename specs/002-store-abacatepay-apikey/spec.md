# Feature Specification: API_KEY do AbacatePay por Loja

**Feature Branch**: `002-store-abacatepay-apikey`
**Created**: 2026-06-26
**Status**: Draft
**Input**: User description: "Implemente a API_KEY do AbacatePay por loja, de forma que eu possa alterar essa API_KEY. A api key deve ficar da tabela da store, mas não pode ser lida pela API, ela só pode ser alterada por um endpoint especifico. Atualize o bruno"

## Clarifications

### Session 2026-06-26

- Q: Como a credencial do AbacatePay deve ser protegida na base de dados (em repouso)? → A: Texto simples (como hoje), sem criptografia adicional; a proteção do segredo se dá pela regra de não-leitura (write-only) na API.
- Q: Quando uma loja não tem credencial própria cadastrada, o que o pagamento deve fazer? → A: Falhar com erro claro; **sem** fallback para a chave global do ambiente.
- Q: A API pode informar se uma loja já tem credencial configurada (sem revelar o valor)? → A: Sim — expor uma flag booleana indicando se a credencial está configurada, nunca o valor.
- Q: No momento de salvar, a credencial deve ser validada contra o AbacatePay? → A: Não validar; apenas armazenar (após normalização de espaços).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Definir/alterar a API_KEY do AbacatePay de uma loja (Priority: P1)

O responsável por uma loja precisa cadastrar (ou trocar) a credencial do AbacatePay que aquela loja usa para processar pagamentos. Hoje a credencial é única para todo o ambiente; cada loja passa a ter a sua própria. O responsável aciona um ponto de entrada dedicado e exclusivo a essa finalidade, informa a nova credencial e ela é armazenada de forma associada à loja, substituindo qualquer valor anterior.

**Why this priority**: É o coração da feature — sem a capacidade de gravar/alterar a credencial por loja, nada mais funciona. Entrega valor por si só, pois permite que cada loja opere com a sua própria conta no AbacatePay.

**Independent Test**: Pode ser testado por completo enviando uma nova credencial para uma loja existente através do ponto de entrada dedicado e confirmando que a operação é aceita e persistida, sem depender de qualquer outra história.

**Acceptance Scenarios**:

1. **Given** uma loja existente da qual sou responsável e que não possui credencial cadastrada, **When** eu envio uma nova credencial pelo ponto de entrada dedicado, **Then** a credencial passa a estar associada à loja e a operação é confirmada como bem-sucedida.
2. **Given** uma loja que já possui uma credencial cadastrada, **When** eu envio uma credencial diferente pelo ponto de entrada dedicado, **Then** o valor anterior é integralmente substituído pelo novo.
3. **Given** uma loja que não me pertence, **When** eu tento alterar a credencial dessa loja, **Then** a operação é recusada e nenhuma alteração é gravada.
4. **Given** uma requisição sem autenticação válida, **When** ela tenta alterar a credencial de qualquer loja, **Then** a operação é recusada.

---

### User Story 2 - Garantir que a credencial nunca seja lida/exposta pela API (Priority: P1)

A credencial do AbacatePay é um segredo sensível. Nenhum consumidor da API (leitura administrativa autenticada, leitura pública ou qualquer retorno de dados da loja) pode obter o valor da credencial — nem em texto claro, nem mascarado, nem por qualquer caminho de consulta. O único caminho permitido para esse dado é a escrita pelo ponto de entrada dedicado da História 1.

**Why this priority**: É um requisito de segurança explícito e inegociável do pedido. Mesmo que a gravação funcione, expor o segredo invalidaria a feature. É independente e testável: basta percorrer todos os caminhos de leitura da loja e verificar a ausência do campo.

**Independent Test**: Consultar a loja por todos os caminhos de leitura disponíveis (consulta administrativa autenticada, consulta/listagem pública e qualquer resposta que devolva dados da loja) e confirmar que o campo da credencial nunca aparece na resposta.

**Acceptance Scenarios**:

1. **Given** uma loja com credencial cadastrada, **When** eu consulto os dados dessa loja pela leitura administrativa autenticada, **Then** o campo da credencial não está presente na resposta.
2. **Given** uma loja com credencial cadastrada, **When** eu consulto os dados dessa loja por qualquer caminho de leitura público, **Then** o campo da credencial não está presente na resposta.
3. **Given** qualquer resposta da API que devolva dados de loja após criar ou atualizar a loja, **When** eu inspeciono a resposta, **Then** o campo da credencial não está presente.
4. **Given** uma loja com credencial cadastrada, **When** eu consulto seus dados por qualquer caminho de leitura, **Then** a resposta pode informar apenas que a credencial está configurada (indicador booleano), mas nunca o valor — nem mascarado.

---

### User Story 3 - Pagamentos da loja usam a credencial da própria loja (Priority: P2)

Ao processar uma cobrança/pagamento via AbacatePay para uma determinada loja, o sistema deve usar a credencial cadastrada para aquela loja, em vez de uma credencial única do ambiente. Assim cada loja transaciona com a sua própria conta no AbacatePay.

**Why this priority**: É o propósito de negócio por trás de armazenar a credencial por loja. Depende da História 1 (a credencial precisa existir) e por isso vem depois, mas é o que materializa o valor da separação por loja.

**Independent Test**: Cadastrar credenciais distintas em duas lojas e disparar operações de pagamento para cada uma, confirmando que cada operação utiliza a credencial correspondente à sua loja.

**Acceptance Scenarios**:

1. **Given** duas lojas com credenciais diferentes cadastradas, **When** uma operação de pagamento é processada para cada uma, **Then** cada operação utiliza a credencial da respectiva loja.
2. **Given** uma loja sem credencial própria cadastrada, **When** uma operação de pagamento é processada para ela, **Then** a operação é recusada com erro claro, sem recorrer a nenhuma credencial padrão do ambiente.

---

### User Story 4 - Coleção Bruno atualizada (Priority: P3)

A coleção de requisições Bruno do repositório deve incluir uma requisição para alterar a credencial do AbacatePay de uma loja, com cabeçalhos e autenticação no mesmo padrão das demais requisições da pasta de loja, de modo que a equipe consiga exercitar o novo ponto de entrada manualmente.

**Why this priority**: É um apoio à equipe/QA e ao uso manual; não bloqueia o funcionamento da feature, por isso prioridade menor. Independentemente testável abrindo a coleção e executando a requisição contra uma loja real.

**Acceptance Scenarios**:

1. **Given** a coleção Bruno do repositório, **When** eu abro a pasta de requisições da loja, **Then** existe uma requisição dedicada para alterar a credencial do AbacatePay da loja.
2. **Given** essa nova requisição, **When** eu a inspeciono, **Then** ela usa os mesmos cabeçalhos de identificação de tenant e de autenticação adotados pelas demais requisições da coleção.

---

### Edge Cases

- **Credencial vazia ou ausente no envio**: o que acontece quando o responsável aciona o ponto de entrada sem informar valor? A operação deve ser rejeitada com mensagem clara, sem apagar silenciosamente um valor previamente cadastrado.
- **Loja inexistente**: alterar a credencial de um identificador de loja que não existe deve resultar em recusa clara, sem criar registros.
- **Espaços/formatação**: valores com espaços em branco nas extremidades devem ser tratados de forma consistente (normalização antes de armazenar) para evitar credencial inválida por engano.
- **Loja sem credencial no momento do pagamento**: a operação de pagamento é recusada com erro claro (sem fallback para credencial de ambiente), conforme História 3 (cenário 2) e FR-014.
- **Concorrência**: duas alterações quase simultâneas da credencial da mesma loja devem resultar em um estado final determinístico (a última gravação prevalece).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST armazenar a credencial do AbacatePay associada a cada loja individualmente, persistida junto aos dados da loja.
- **FR-002**: O sistema MUST oferecer um ponto de entrada dedicado e exclusivo para definir/alterar a credencial do AbacatePay de uma loja.
- **FR-003**: A alteração da credencial MUST exigir autenticação válida e MUST ser permitida apenas ao responsável pela loja em questão.
- **FR-004**: Ao receber um novo valor, o sistema MUST substituir integralmente a credencial anterior daquela loja.
- **FR-005**: O sistema MUST NOT expor o valor da credencial em nenhuma resposta de leitura da API — incluindo leitura administrativa autenticada, leitura pública e quaisquer respostas que retornem dados da loja (por exemplo, após criação ou atualização da loja). É permitido expor apenas um indicador booleano de que a credencial está configurada (ver FR-013).
- **FR-006**: O sistema MUST NOT permitir a obtenção da credencial por nenhum caminho que não seja a própria escrita pelo ponto de entrada dedicado (o dado é de escrita apenas — write-only).
- **FR-007**: Os pontos de entrada de criação e atualização geral da loja MUST NOT aceitar a credencial como parâmetro, mantendo a alteração restrita ao ponto de entrada dedicado.
- **FR-008**: O sistema MUST rejeitar uma alteração cujo valor de credencial seja vazio ou ausente, com mensagem clara, e sem apagar um valor já existente.
- **FR-009**: O sistema MUST rejeitar de forma clara uma tentativa de alteração para uma loja inexistente, sem criar registros.
- **FR-010**: Ao processar operações de pagamento via AbacatePay para uma loja, o sistema MUST utilizar a credencial cadastrada para aquela loja.
- **FR-011**: O isolamento por tenant MUST ser preservado — a credencial de uma loja de um tenant não pode ser acessada nem afetada a partir de outro tenant.
- **FR-012**: A coleção Bruno do repositório MUST ser atualizada com uma requisição para o ponto de entrada de alteração da credencial, alinhada ao padrão de cabeçalhos e autenticação já usado na coleção.
- **FR-013**: O sistema MAY expor, nas leituras de dados da loja, um indicador booleano sinalizando se a credencial está configurada para aquela loja; esse indicador MUST NOT revelar o valor (nem mesmo mascarado).
- **FR-014**: Quando uma loja não possuir credencial própria cadastrada, o sistema MUST recusar as operações de pagamento dessa loja com erro claro e MUST NOT recorrer a nenhuma credencial padrão do ambiente como alternativa.
- **FR-015**: O sistema MUST armazenar a credencial em texto simples (sem criptografia adicional em repouso); a proteção do segredo se apoia exclusivamente na regra de não-leitura pela API e no controle de acesso ao banco de dados.
- **FR-016**: O sistema MUST NOT realizar validação on-line da credencial junto ao AbacatePay no ato da gravação; o valor é apenas normalizado (remoção de espaços nas extremidades) e armazenado.

### Key Entities *(include if feature involves data)*

- **Loja (Store)**: representa um comerciante/estabelecimento dentro de um tenant. Passa a possuir um atributo adicional — a credencial do AbacatePay — que é sensível, de escrita apenas e nunca retornado em leituras. Demais atributos da loja (nome, e-mail, identificador, responsável, estratégia de cobrança) permanecem inalterados.
- **Credencial do AbacatePay**: segredo que autoriza a loja a transacionar na conta AbacatePay correspondente. É atributo da Loja; alterável somente pelo ponto de entrada dedicado; nunca legível pela API.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos caminhos de leitura de dados de loja (administrativo, público e respostas de criação/atualização) não contêm o valor da credencial em nenhuma forma.
- **SC-002**: O responsável por uma loja consegue definir ou alterar a credencial em uma única operação pelo ponto de entrada dedicado.
- **SC-003**: 100% das tentativas de alteração feitas sem autenticação válida ou por quem não é responsável pela loja são recusadas.
- **SC-004**: Para duas lojas com credenciais distintas, 100% das operações de pagamento usam a credencial correspondente à loja da operação.
- **SC-005**: A coleção Bruno contém a requisição de alteração da credencial e ela executa com sucesso contra uma loja real existente.
- **SC-006**: Nenhuma loja de um tenant consegue ler ou alterar a credencial de uma loja de outro tenant.
- **SC-007**: 100% das operações de pagamento para lojas sem credencial própria são recusadas com erro claro, sem usar qualquer credencial de ambiente.
- **SC-008**: As leituras de loja conseguem indicar corretamente se a credencial está ou não configurada, sem em nenhum caso retornar o valor.

## Assumptions

- **Autorização**: a alteração da credencial segue o mesmo modelo de autorização já usado nos demais pontos de entrada de loja — requer autenticação e que o solicitante seja o responsável (owner) da loja.
- **Sem fallback para credencial de ambiente**: quando uma loja não tiver credencial própria cadastrada, as operações de pagamento dessa loja são recusadas com erro claro (FR-014); o sistema não recorre à credencial global do ambiente. Lojas em produção precisarão cadastrar sua credencial por loja antes de continuar transacionando.
- **Armazenamento em texto simples**: a credencial é gravada sem criptografia adicional em repouso (FR-015); a confidencialidade depende da regra de não-leitura pela API e do controle de acesso ao banco.
- **Indicador de configuração**: as leituras da loja podem expor apenas um booleano indicando se a credencial está configurada (FR-013), nunca o valor.
- **Validação externa no momento da gravação**: a credencial é armazenada como fornecida (após normalização de espaços), sem validação on-line contra o AbacatePay no ato da alteração (FR-016); valores inválidos só se manifestam na primeira operação de pagamento.
- **Sem histórico/versão**: apenas o valor corrente da credencial é mantido; não há histórico de credenciais anteriores.
- **Limpeza de credencial**: remover/limpar uma credencial já existente não faz parte do escopo desta entrega — o ponto de entrada serve para definir/substituir por um novo valor não vazio.
- **Persistência já preparada**: a estrutura de dados da loja já comporta o armazenamento da credencial; esta feature passa a utilizá-la efetivamente.

## Dependencies

- Mecanismo de autenticação e de identificação do responsável pela loja já existente no produto.
- Isolamento por tenant já existente (cada tenant com sua própria base de dados).
- Integração de pagamentos com o AbacatePay já existente, que passará a considerar a credencial por loja.
