# Abertura do OS Service

Esta etapa cria o primeiro limite de microsservico sem quebrar o contrato existente de `POST /api/v1/ordens-servico`.

## Contrato preservado

- cliente e veiculo sao obrigatorios, devem existir e o veiculo deve pertencer ao cliente;
- nao pode existir outra OS ativa para o mesmo cliente e veiculo;
- `descricaoSolicitacao` continua obrigatoria;
- `servicos` continua obrigatorio e deve ter ao menos um ID sem duplicidade;
- `pecas` continua opcional; quantidades repetidas sao acumuladas;
- servicos e pecas inexistentes continuam retornando erro;
- preco e tempo estimado ficam registrados na OS como snapshot;
- a abertura nao consulta disponibilidade, reserva ou baixa estoque.

## Limite introduzido

```text
CriarOrdemDeServicoCommandHandler
    -> ICatalogoExecucao (Application)
        -> CatalogoExecucaoHttpClient (Infrastructure)
            -> Execucao API /api/v1/catalogo/itens/consultar
```

O handler deixou de depender diretamente dos repositorios de servicos e pecas. O cliente HTTP consulta os dois tipos de item em lote, com timeout, retry e circuit breaker. O adaptador local permanece apenas na fabrica dos testes de integracao do legado.

O retorno do catalogo contem somente ID, nome, preco e tempo estimado. O OS Service nao recebe saldo de estoque. Todas as gravacoes locais da abertura, incluindo itens, historico e notificacao, executam na mesma transacao.

## Limites desta etapa

Esta implementacao ja consulta o banco proprio da Execucao por HTTP. As tabelas e chaves estrangeiras legadas de servico e peca ainda continuam no schema do OS porque as jornadas de diagnostico e estoque ainda dependem delas. Idempotencia, outbox e eventos da Saga entram nos proximos cortes.

O proximo passo e extrair diagnostico e estoque para a Execucao e entao remover as dependencias relacionais restantes entre a OS e o catalogo.
