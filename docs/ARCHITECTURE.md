# Arquitetura

## Visão geral

A solução é um monólito em camadas orientado a casos de uso. O domínio principal é a ordem de serviço, integrado ao controle de estoque, movimentações e pedidos de compra para suportar o avanço seguro dos status da OS.

## Camadas

```text
API
  controllers, filtros e contrato HTTP

Application
  serviços de aplicação, DTOs e validações

Domain
  entidades, enums e regras de negócio

Infrastructure
  EF Core, repositórios, migrations, seed e extensões de startup
```

## Estrutura principal

```text
src/
  API/
    Controllers/
    Filters/
  Application/
    DTOs/
    Options/
    Security/
    Services/
    Validators/
  Domain/
    Entities/
    Enums/
    Repositories/
  Infrastructure/
    Data/
    Extensions/
    HealthChecks/
    Migrations/
    Repositories/
  Shared/
    Helpers/
    Logging/
```

## Componentes centrais

### API

Responsabilidades:

- expor endpoints REST;
- aplicar autenticação e autorização;
- receber DTOs;
- traduzir exceções de domínio em resposta HTTP.

Pontos relevantes:

- `Program.cs` registra DI, JWT, EF Core, Swagger, CORS e health checks;
- `DomainExceptionFilter` converte erros previsíveis de domínio em `400` e `404`;
- controllers delegam a lógica para `Application.Services`.

### Application

Responsabilidades:

- implementar casos de uso;
- consultar e persistir dados via contratos de repositório;
- coordenar transações entre OS, estoque, histórico e compras;
- montar DTOs de saída;
- registrar logs.

Principais serviços:

- `ClienteApplicationService`
- `VeiculoApplicationService`
- `ServicoApplicationService`
- `PecaApplicationService`
- `OrdemDeServicoApplicationService`
- `PedidoCompraApplicationService`
- `AuthApplicationService`
- `AcompanhamentoOSApplicationService`

### Domain

Responsabilidades:

- encapsular regras do negócio;
- controlar transições válidas de status;
- manter consistência do agregado de ordem de serviço;
- impedir execução da OS sem estoque validado.

Entidades centrais:

- `Cliente`
- `Veiculo`
- `Servico`
- `Peca`
- `OrdemDeServico`
- `OrdemServicoHistorico`
- `PedidoCompra`
- `MovimentacaoEstoque`
- `NotificacaoCliente`
- `Usuario`

`OrdemDeServico` concentra:

- fluxo de status;
- composição do orçamento;
- histórico da ordem;
- validações operacionais;
- bloqueio por falta de estoque.

Fluxo implementado:

```text
Recebida -> EmDiagnostico -> AguardandoAprovacao -> EmExecucao -> Finalizada -> Entregue
                                   \-> AguardandoEstoque -> EmExecucao
```

Regras relevantes:

- serviços só podem ser adicionados ou removidos em `EmDiagnostico`;
- peças podem ser adicionadas em `EmDiagnostico`, `AguardandoAprovacao` e `AguardandoEstoque`;
- peças só podem ser removidas em `EmDiagnostico`;
- a transição para `EmExecucao` depende de validação de estoque com sucesso;
- se faltar estoque, a OS vai para `AguardandoEstoque` e pode gerar pedido de compra.

### Infrastructure

Responsabilidades:

- mapear entidades com Entity Framework Core;
- implementar repositórios concretos;
- manter migrations;
- executar seed de desenvolvimento;
- aplicar startup tasks como migration automática.

Pontos relevantes:

- `OficinaDbContext` centraliza o modelo relacional;
- `HostExtensions.MigrateAndSeedAsync` aplica migrations com retry;
- `OficinaDbContextSeeder` cria dados base para desenvolvimento;
- `EfTransactionManager` garante consistência entre atualizações relacionadas.

## Fluxo de requisição

```text
HTTP Request
  -> Controller
  -> Application Service
  -> Domain Entity / Business Rule
  -> Repository
  -> DbContext / PostgreSQL
  -> DTO de resposta
  -> HTTP Response
```

## Persistência

Banco principal:

- PostgreSQL 16

Justificativa da escolha do PostgreSQL:

O PostgreSQL foi escolhido por ser um banco relacional robusto, open source e amplamente adotado em aplicações transacionais. O domínio da oficina mecânica possui relacionamentos fortes entre clientes, veículos, ordens de serviço, serviços, peças, movimentações de estoque, notificações e pedidos de compra. Por isso, o modelo relacional atende bem à necessidade de integridade, consistência e rastreabilidade das operações.

A escolha também favorece o controle transacional exigido pelo fluxo da ordem de serviço. Operações como liberar uma OS para execução, validar estoque, registrar movimentações e gerar pedidos de compra precisam manter os dados consistentes mesmo quando envolvem múltiplas tabelas. O PostgreSQL oferece suporte maduro a transações ACID, chaves estrangeiras, índices, constraints e consultas relacionais, recursos importantes para esse tipo de regra de negócio.

Outro ponto considerado foi a integração com a stack do projeto. O PostgreSQL possui excelente suporte no Entity Framework Core por meio do provider Npgsql, funciona bem em ambientes Docker e permite que o projeto seja executado localmente com baixo custo de infraestrutura. Assim, a escolha equilibra confiabilidade, facilidade de desenvolvimento, portabilidade e aderência ao cenário de uma API de gestão operacional.

Banco para testes:

- EF Core InMemory quando `Environment=Testing` ou connection string `UseInMemory`

Migrations:

- mantidas em `src/Infrastructure/Migrations`
- aplicadas automaticamente no startup fora do ambiente de testes

## Segurança

Autenticação:

- JWT Bearer
- emissão de token por `AuthApplicationService`

Estado atual da autorização:

- protegidos: `Clientes`, `Veiculos`, `Servicos`, `Pecas`, `OrdensDeServico`, `PedidosCompra`
- públicos: `Auth` e acompanhamento público de OS em `AcompanhamentoOS`

## Observabilidade

Health checks expostos:

- `/health`
- `/health/live`
- `/health/ready`

Logging:

- console logging;
- template padronizado nas principais operações de aplicação e startup.

## Testabilidade

O projeto possui:

- testes unitários focados em serviços de aplicação;
- testes de integração focados em controllers;
- suporte a banco em memória para cenários de teste.
