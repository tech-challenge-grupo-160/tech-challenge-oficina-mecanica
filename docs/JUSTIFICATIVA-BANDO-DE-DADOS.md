# Justificativa Técnica: Escolha do Banco de Dados, Evolução do Modelo Relacional e Diagrama ER

**Projeto:** Sistema de Gestão de Oficina Mecânica
**Escopo do documento:** Justificativa formal da escolha do SGBD, análise dos ajustes realizados no modelo relacional ao longo do projeto e explicação detalhada do Diagrama Entidade-Relacionamento (DER) e seus relacionamentos.

---

## 1. Justificativa da Escolha do Banco de Dados

### 1.1 Decisão adotada

O sistema utiliza **PostgreSQL 16**, hospedado como instância gerenciada **Amazon RDS (Single-AZ, com backup automatizado)**, acessado pela API .NET através do **Entity Framework Core** com o provider **Npgsql**.

### 1.2 Natureza do domínio: por que um banco relacional

O domínio da Oficina Mecânica é fundamentalmente **transacional e fortemente relacional**:

- Uma Ordem de Serviço (OS) referencia obrigatoriamente um `Cliente` e um `Veículo`, que por sua vez se relacionam entre si (um cliente possui vários veículos).
- Peças e serviços são compartilhados por múltiplas ordens de serviço (relações N:N via `OrdemServicoItemPeca` e `OrdemServicoItemServico`).
- O controle de estoque (`MovimentacaoEstoque`, `PedidoCompra`) depende de **integridade referencial rígida** entre peça, ordem de serviço e pedido de compra — um erro de referência nesse ponto gera inconsistência financeira e operacional (estoque "fantasma", peças cobradas sem lastro).
- Há exigência de **consistência transacional (ACID)** em operações como: baixa de estoque + registro de movimentação + vínculo com a OS, que precisam ocorrer de forma atômica — se qualquer etapa falhar, nenhuma deve ser persistida.

Esse conjunto de características — muitas entidades interligadas, integridade referencial como regra de negócio (não apenas validação de aplicação) e necessidade de transações atômicas multi-tabela — é o caso de uso clássico para um **SGBD relacional (RDBMS)**, e não para bancos NoSQL orientados a documento ou chave-valor, que sacrificam justamente essas garantias em favor de escalabilidade horizontal e schema flexível — características que este domínio não demanda.

### 1.3 Por que PostgreSQL especificamente (e não outro RDBMS)

| Critério | Justificativa |
|---|---|
| **Conformidade ACID robusta** | PostgreSQL possui implementação MVCC madura, garantindo isolamento de transações concorrentes — essencial quando múltiplos atendentes podem atualizar a mesma OS ou o mesmo estoque de peça simultaneamente. |
| **Constraints e integridade nativa** | Suporte completo a chaves estrangeiras com ações declarativas (`RESTRICT`, `CASCADE`, `SET NULL`), índices únicos compostos e parciais — usados extensivamente no schema (ex.: `IX_Cliente_CpfCnpj`, `IX_OrdemServico_Numero`, `IX_OrdemServico_CodigoAcompanhamento`, todos únicos). |
| **Tipos de dados adequados ao domínio** | `numeric(18,2)` para valores monetários (evita erros de arredondamento de ponto flutuante em `Preco`, `ValorTotal`), `timestamp` para auditoria de eventos, `text` para descrições longas. |
| **Estratégias de geração de identidade flexíveis** | Uso de `IdentitySequenceOptions` (Npgsql) permitiu, por exemplo, reiniciar a sequência de `OrdemServico.Id` em `3000` após uma correção de dados de seed (migration `AddDadosRecepcaoOrdemServico`), sem necessidade de recriar a tabela. |
| **Custo e licenciamento** | Open source, sem custos de licenciamento por núcleo/instância, o que é relevante em um projeto acadêmico/MVP com orçamento AWS limitado (RDS Single-AZ, `db.t3` ou similar). |
| **Maturidade do provider .NET (Npgsql)** | Integração de primeira classe com Entity Framework Core, incluindo suporte a migrations versionadas — evidenciado pelo próprio histórico de migrations do projeto, que documenta cada alteração de schema de forma rastreável e reversível (`Up`/`Down`). |
| **Alinhamento com a infraestrutura de nuvem** | Amazon RDS oferece PostgreSQL como serviço gerenciado de primeira classe (patching, backup automatizado, monitoramento via CloudWatch/Datadog), reduzindo a carga operacional da equipe — coerente com a arquitetura de deployment adotada (Fase 3, `RDS PostgreSQL [Single-AZ, backup automatizado]`). |

### 1.4 Por que não NoSQL

Um banco orientado a documentos (ex.: DynamoDB, MongoDB) foi descartado porque:

1. **Consultas relacionais multi-tabela são centrais ao domínio** — por exemplo, listar ordens de serviço filtrando por cliente, status e intervalo de datas (`ListarOrdensDeServicoQuery`), ou consolidar o histórico + notificações + movimentações de estoque de uma OS — exigem `JOIN`s naturais que um modelo desnormalizado tornaria custosos ou replicaria dados de forma arriscada (risco de inconsistência entre cópias).
2. **Integridade referencial como regra de negócio, não apenas validação de aplicação** — nesse domínio, é inaceitável que uma `OrdemServicoItemPeca` referencie uma peça inexistente, ou que uma `MovimentacaoEstoque` fique órfã. Bancos NoSQL não impõem isso nativamente; a responsabilidade recairia inteiramente sobre a camada de aplicação, aumentando o risco de inconsistência.
3. **Volume e padrão de acesso não justificam a troca** — o sistema não tem requisitos de escala horizontal massiva (milhões de escritas/segundo) que tipicamente motivam a escolha de NoSQL; o padrão de acesso é OLTP clássico de baixo/médio volume.

---

## 2. Ajustes Realizados no Modelo Relacional (Evolução via Migrations)

O modelo relacional não foi definido de uma única vez: evoluiu de forma incremental, documentada em 13 migrations do Entity Framework Core. Essa evolução reflete decisões de design deliberadas, correções de rota e reforços de segurança. Abaixo, a justificativa de cada ajuste relevante, agrupada por motivação.

### 2.1 Modelagem inicial (`InitialCreateIntIdentity`)

- **Chave primária `int` com `IDENTITY`, e não `GUID`**: optou-se por inteiros sequenciais em vez de UUIDs para reduzir o tamanho dos índices (B-tree mais compactas e eficientes) e favorecer a legibilidade de identificadores expostos operacionalmente (ex.: número de OS). O custo de um `int` sequencial — previsibilidade da sequência — não é um problema de segurança relevante aqui, pois o acesso público (clientes) é feito por um código opaco separado (`CodigoAcompanhamento`), não pelo `Id` interno.
- **`OnDelete: Restrict` como padrão para entidades "mestre"** (`Cliente`, `Veiculo`, `Peca`, `Servico`): impede a exclusão acidental de um registro ainda referenciado por uma OS, protegendo o histórico transacional.

### 2.2 Enriquecimento de dados de recepção e cancelamento

- `AddDadosRecepcaoOrdemServico` adicionou `DescricaoSolicitacao` (obrigatório) e `ObservacoesRecepcao` (opcional). A estratégia de migração foi cuidadosa: a coluna foi criada com `defaultValue: ""` para não quebrar linhas existentes, seguida de um `UPDATE` para preencher um valor significativo (`'Solicitacao nao informada.'`) nas linhas legadas — um padrão seguro de migração *add-then-backfill* que evita indisponibilidade ou falha de constraint em dados já persistidos.
- Na mesma migration, a sequência de `OrdemServico.Id` foi reiniciada em `3000` (`ALTER COLUMN "Id" RESTART WITH 3000`), sinalizando uma reorganização de dados de seed/teste sem impacto em produção.
- `AddCancelamentoOrdemServico` adicionou `MotivoCancelamento` (nullable), suportando o fluxo de cancelamento sem forçar preenchimento em ordens não canceladas.

### 2.3 Refino iterativo da máquina de estados (Status da OS)

Duas migrations documentam um ajuste e posterior **reversão** do fluxo de orçamento:

- `AdjustOrdemDeServicoStatusFluxoOrcamento`: incrementou em 1 todos os status `>= 2`, abrindo espaço para um novo status intermediário no fluxo de orçamento.
- `RevertFluxoOrcamentoEmProcesso`: reverteu parcialmente essa decisão, unificando o status `2` de volta ao `1` e decrementando os status seguintes.

Esse par de migrations é evidência de um processo de design **iterativo e adaptativo**: uma tentativa de detalhar mais o fluxo de orçamento (provavelmente um status "orçamento em processo") foi implementada, avaliada e revertida quando se concluiu que a granularidade extra não agregava valor ao negócio — mantendo o modelo mais simples. É um exemplo de correção de rota registrada de forma rastreável e reversível, em vez de "silenciosamente" sobrescrita.

- `AddOrcamentoEnviadoEmToOrdemServico` e `AddDatasFinalizacaoEPagamentoOrdemServico` complementam o ciclo de vida da OS com timestamps de auditoria (`OrcamentoEnviadoEm`, `DataFinalizacao`, `DataPagamento`), permitindo reconstruir a linha do tempo completa de uma ordem sem depender apenas do campo `Status` (que é mutável e não preserva histórico por si só).

### 2.4 Introdução de auditoria e rastreabilidade (`AddOrdemServicoHistorico`)

Criou-se uma tabela dedicada de histórico (`OrdemServicoHistorico`) em vez de sobrescrever o status na própria `OrdemServico`. Essa é uma decisão de normalização deliberada: **o estado atual e o histórico de eventos são responsabilidades de tabelas diferentes**, evitando que a tabela principal cresça descontroladamente com campos de auditoria e permitindo consultas de histórico (`GET /ordens-servico/{id}/historico`) sem impacto na tabela transacional principal.

### 2.5 Gestão de estoque como sistema de "livro-razão" (`AddGestaoPecasEInsumosOrdemServico`)

Em vez de apenas decrementar/incrementar `Peca.QuantidadeEstoque` diretamente, o modelo introduziu duas tabelas:

- `PedidoCompra`: registra solicitações de reposição de peça vinculadas a uma OS específica (rastreando o motivo da compra).
- `MovimentacaoEstoque`: funciona como um **livro-razão de estoque** (ledger), registrando `QuantidadeAnterior` e `QuantidadePosterior` a cada movimentação, com referência opcional tanto à OS de origem quanto ao pedido de compra que a gerou.

Esse é um trade-off consciente de **complexidade adicional em troca de auditabilidade**: qualquer discrepância de estoque pode ser reconstruída a partir do histórico de movimentações, em vez de depender apenas do valor corrente da coluna `QuantidadeEstoque`.

### 2.6 Enriquecimento de catálogo (`AddMarcaEModeloPeca`)

Adicionou `Marca` e `Modelo` à tabela `Peca`, com backfill via `CASE` SQL mapeando nomes de peças legadas para marca/modelo plausíveis — outro exemplo do padrão *add-then-backfill* usado consistentemente no projeto para evitar dados nulos/vazios após alterações estruturais.

### 2.7 Comunicação com o cliente como entidade própria (`AddNotificacaoCliente`)

Assim como o histórico, as notificações enviadas ao cliente foram modeladas como tabela independente (`NotificacaoCliente`, 1:N a partir de `OrdemServico`), e não como campos na própria OS — permitindo múltiplos envios por canais diferentes (`Canal`) sem limitar a cardinalidade nem poluir a entidade principal.

### 2.8 Acesso público seguro (`AddAcompanhamentoPublicoOs`)

Adicionou `CodigoAcompanhamento` (identificador público, opaco, não sequencial) e `TokenAcompanhamentoHash` (hash do token de acesso, nunca o token em texto puro) à `OrdemServico`, ambos com índice único. Essa é uma decisão de **segurança por design**: o cliente final acessa o status da sua OS por um código não previsível, e o token de validação é armazenado apenas como hash — mesmo em caso de vazamento do banco, o token original não é recuperável diretamente.

### 2.9 Correção de segurança crítica (`ReplaceMd5PasswordHashWithBCrypt`)

A migration mais recente substitui o algoritmo de hash de senha de **MD5 para BCrypt**, incluindo:

- Ampliação da coluna `SenhaHash` de `varchar(64)` para `varchar(255)` (necessária pois hashes BCrypt são mais longos que hashes MD5 hexadecimais).
- Atualização condicional do hash do usuário `admin` apenas se o valor atual ainda correspondesse ao hash MD5 legado conhecido — evitando sobrescrever senhas já alteradas por outro processo.

Essa mudança é significativa do ponto de vista de segurança: **MD5 é criptograficamente quebrado e inadequado para hashing de senhas** (rápido de forçar bruta e vulnerável a rainbow tables); **BCrypt é um algoritmo de hashing lento e com salt embutido**, desenhado especificamente para esse propósito. A migration documenta uma correção de débito técnico de segurança de forma auditável e reversível (o `Down` reverte para MD5, preservando a capacidade de rollback, embora a reversão não seja recomendada em produção).

### 2.10 Síntese dos ajustes

| Categoria de ajuste | Exemplos | Motivação |
|---|---|---|
| Evolução funcional | `AddDadosRecepcaoOrdemServico`, `AddCancelamentoOrdemServico`, `AddDatasFinalizacao...` | Cobrir etapas do fluxo de negócio não previstas na modelagem inicial |
| Correção de rota | `AdjustOrdemDeServicoStatusFluxoOrcamento` + `RevertFluxoOrcamentoEmProcesso` | Ajuste e reversão de uma decisão de design após avaliação |
| Normalização/auditoria | `AddOrdemServicoHistorico`, `AddNotificacaoCliente`, `AddGestaoPecasEInsumosOrdemServico` | Separar estado atual de histórico de eventos; rastreabilidade |
| Enriquecimento de catálogo | `AddMarcaEModeloPeca` | Melhorar qualidade/detalhe dos dados de peças |
| Segurança | `AddAcompanhamentoPublicoOs`, `ReplaceMd5PasswordHashWithBCrypt` | Acesso público seguro por token hasheado; hashing de senha adequado |

---

## 3. Diagrama ER e Explicação dos Relacionamentos

*(referência: diagrama `oficina-mecanica-er-diagram`, entregue anteriormente)*

### 3.1 Entidades "mestre" (dados cadastrais)

| Entidade | Papel no modelo |
|---|---|
| `Cliente` | Identificado unicamente por `CpfCnpj` (índice único) — chave de negócio usada tanto no atendimento presencial quanto na autenticação pública por CPF. |
| `Veiculo` | Identificado unicamente por `Placa`. Pertence a exatamente um `Cliente`. |
| `Peca` | Catálogo de peças, com preço e estoque corrente. |
| `Servico` | Catálogo de serviços oferecidos, com preço e tempo estimado. |
| `Usuario` | Tabela isolada, sem relacionamento com o restante do domínio — usada exclusivamente para autenticação de **funcionários** (login/senha/role). Não participa de nenhuma FK do domínio de negócio. |

### 3.2 Relacionamentos 1:N e suas regras de exclusão

| Relacionamento | Cardinalidade | `ON DELETE` | Justificativa da regra |
|---|---|---|---|
| `Cliente` → `Veiculo` | 1:N | `RESTRICT` | Um cliente pode ter vários veículos; não é possível excluir um cliente que ainda possui veículo cadastrado — evita perda de rastreabilidade histórica. |
| `Cliente` → `OrdemServico` | 1:N | `RESTRICT` | Protege o histórico financeiro/operacional: uma OS sempre precisa apontar para um cliente válido. |
| `Veiculo` → `OrdemServico` | 1:N | `RESTRICT` | Mesma lógica: a OS é indissociável do veículo atendido. |
| `OrdemServico` → `OrdemServicoHistorico` | 1:N | `CASCADE` | O histórico só faz sentido no contexto da OS; se a OS for removida (cenário de teste/expurgo), o histórico associado é removido junto. |
| `OrdemServico` → `PedidoCompra` | 1:N | `CASCADE` | Pedido de compra é sempre motivado por uma OS específica; sem a OS, o pedido perde seu contexto de negócio. |
| `OrdemServico` → `NotificacaoCliente` | 1:N | `CASCADE` | Notificações são um subproduto da existência da OS. |
| `OrdemServico` → `MovimentacaoEstoque` | 0..1:N (FK nullable) | `CASCADE` | Nem toda movimentação de estoque decorre de uma OS (pode ser ajuste manual/recebimento avulso) — por isso a FK é opcional; quando presente, segue o ciclo de vida da OS. |
| `PedidoCompra` → `MovimentacaoEstoque` | 0..1:N (FK nullable) | `SET NULL` | Diferente do caso acima: se o pedido de compra for removido, a movimentação de estoque **não deve ser apagada** (ela já ocorreu fisicamente) — apenas perde a referência ao pedido que a originou. Essa é uma distinção intencional entre "dado que só existe por causa do pai" (`CASCADE`) e "dado que é factual e deve sobreviver mesmo sem o pai" (`SET NULL`). |
| `Peca` → `PedidoCompra`, `Peca` → `MovimentacaoEstoque` | 1:N | `RESTRICT` | Uma peça não pode ser excluída enquanto houver pedidos ou movimentações vinculadas — preserva a integridade do histórico de estoque. |

### 3.3 Relacionamentos N:N (tabelas associativas)

| Relacionamento | Tabela associativa | Atributos extras | Justificativa |
|---|---|---|---|
| `OrdemServico` ↔ `Peca` | `OrdemServicoItemPeca` (PK composta) | `Quantidade`, `Preco` | O campo `Preco` armazena o valor da peça **no momento em que foi utilizada na OS** (snapshot de preço) — isso é essencial: se o preço de catálogo da peça mudar depois, o valor cobrado na OS já fechada não deve ser recalculado retroativamente. |
| `OrdemServico` ↔ `Servico` | `OrdemServicoItemServico` (PK composta) | `Preco`, `TempoEstimadoMinutos` | Mesma lógica de *snapshot*: preserva o preço e tempo estimado vigentes no momento da execução, desacoplando o histórico de OS de alterações futuras no catálogo de serviços. |

Em ambos os casos, a exclusão em cascata a partir da `OrdemServico` (`CASCADE`) remove os itens associados junto com a OS, enquanto a exclusão a partir de `Peca`/`Servico` é restrita (`RESTRICT`) — protegendo o catálogo de ser apagado enquanto referenciado por qualquer item de OS já registrado.

### 3.4 Caso particular: `Usuario` sem relacionamento

Conforme identificado anteriormente na análise do modelo, `Usuario` não possui nenhuma FK de saída nem é referenciada por nenhuma outra tabela via constraint formal. O campo `OrdemServicoHistorico.UsuarioId` é armazenado como `varchar(100)` (não como FK `int` para `Usuario.Id`), o que indica uma **ligação lógica, não física** — provavelmente o identificador do usuário autenticado é obtido da claim do token JWT na camada de aplicação, e gravado como texto no histórico para fins de auditoria, sem constraint de integridade referencial no banco.

### 3.5 Padrões de design evidenciados no DER

1. **Separação entre estado atual e histórico** (`OrdemServico.Status` vs. `OrdemServicoHistorico`): favorece consultas rápidas sobre o estado corrente sem sacrificar auditabilidade.
2. **Snapshot de preço em relações N:N** (`OrdemServicoItemPeca`, `OrdemServicoItemServico`): protege o histórico financeiro de alterações futuras no catálogo.
3. **Livro-razão de estoque** (`MovimentacaoEstoque`): substitui a simples atualização de um contador por um registro imutável e reconstituível de cada movimentação.
4. **Uso diferenciado de `CASCADE`, `RESTRICT` e `SET NULL`**: cada ação de exclusão reflete uma regra de negócio específica sobre o que é "dado dependente que morre com o pai" versus "dado mestre que deve ser protegido" versus "dado factual que sobrevive à perda da referência".
5. **Segurança por design em dados sensíveis**: hash de senha com BCrypt e hash de token de acompanhamento público, evitando armazenar segredos em texto puro mesmo internamente.

---

## 4. Conclusão

A escolha do PostgreSQL como SGBD foi orientada pela natureza fortemente relacional e transacional do domínio de negócio, pela necessidade de integridade referencial como regra de negócio (e não apenas validação de aplicação) e pela maturidade de sua integração com o ecossistema .NET/EF Core adotado no projeto. A evolução do modelo relacional, documentada de forma granular e reversível através de 13 migrations, demonstra um processo de design **incremental e responsivo**: da modelagem inicial simples, passando por ajustes e reversões de fluxo de negócio, até a introdução de padrões de auditoria (histórico, notificações, livro-razão de estoque) e correções de segurança (hashing de senha, tokens de acesso público). O diagrama ER resultante reflete essas decisões através do uso deliberado e diferenciado das ações de exclusão (`RESTRICT`, `CASCADE`, `SET NULL`) e de padrões como o *snapshot* de preços nas tabelas associativas — escolhas de modelagem que priorizam a integridade e a rastreabilidade do histórico operacional e financeiro da oficina.
