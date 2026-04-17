# Arquitetura

## Visão geral

A solução é um monólito em camadas orientado a casos de uso. O projeto separa responsabilidades entre API, aplicação, domínio e infraestrutura, mantendo o domínio de oficina mecânica desacoplado dos detalhes de persistência e entrega HTTP.

## Camadas

```text
API
  recebe requisições HTTP, autentica, valida e devolve respostas

Application
  orquestra casos de uso, converte DTOs e coordena repositórios

Domain
  define entidades, regras de negócio e contratos

Infrastructure
  implementa persistência, migrations, seed, health checks e integração com PostgreSQL
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
    Services/
    Validators/
  Domain/
    Entities/
    Repositories/
  Infrastructure/
    Data/
    Extensions/
    HealthChecks/
    Migrations/
    Repositories/
  Shared/
    Helpers/
```

## Componentes centrais

### API

Responsabilidades:

- expor endpoints REST;
- aplicar autenticação e autorização;
- receber DTOs de entrada;
- traduzir exceções de domínio para códigos HTTP.

Pontos relevantes:

- `Program.cs` registra DI, EF Core, JWT, Swagger, CORS e health checks;
- `DomainExceptionFilter` transforma `KeyNotFoundException` em `404` e exceções de argumento/operação em `400`;
- os controllers delegam a lógica para `Application.Services`.

### Application

Responsabilidades:

- implementar os casos de uso da aplicação;
- validar entradas com FluentValidation;
- interagir com contratos de repositório;
- montar DTOs de resposta.

Principais serviços:

- `ClienteApplicationService`
- `VeiculoApplicationService`
- `ServicoApplicationService`
- `PecaApplicationService`
- `OrdemDeServicoApplicationService`
- `AuthApplicationService`

### Domain

Responsabilidades:

- representar o modelo do negócio;
- encapsular regras de transição e consistência;
- definir contratos de persistência.

Entidades principais:

- `Cliente`
- `Veiculo`
- `Servico`
- `Peca`
- `OrdemDeServico`
- `Usuario`

`OrdemDeServico` é o agregado mais importante do domínio. Ele controla:

- transição de status;
- adição de serviços;
- adição de peças com baixa de estoque;
- recálculo do valor total.

Fluxo de status implementado:

```text
Recebida -> EmDiagnostico -> AguardandoAprovacao -> EmExecucao -> Finalizada -> Entregue
```

### Infrastructure

Responsabilidades:

- implementar repositórios com Entity Framework Core;
- mapear entidades no `OficinaDbContext`;
- aplicar migrations;
- popular dados iniciais em desenvolvimento;
- expor health checks.

Pontos relevantes:

- `OficinaDbContext` centraliza o acesso ao banco;
- `HostExtensions.MigrateAndSeedAsync` executa migration com retry e seed em desenvolvimento;
- `OficinaDbContextSeeder` cria dados iniciais para uso local e testes manuais.

## Fluxo de requisição

```text
HTTP Request
  -> Controller
  -> Application Service
  -> Entidades / regras de domínio
  -> Repositório
  -> DbContext / PostgreSQL
  -> DTO de resposta
  -> HTTP Response
```

## Persistência e inicialização

O projeto usa PostgreSQL como banco principal. Em ambiente `Testing`, ou quando a connection string é `UseInMemory`, a aplicação usa banco em memória.

No startup:

1. a aplicação registra dependências e middleware;
2. configura autenticação JWT;
3. configura o `DbContext`;
4. publica health checks;
5. executa migrations automaticamente;
6. executa seed apenas em ambiente `Development`.

## Segurança

Autenticação:

- JWT Bearer;
- configuração em `Jwt`;
- token emitido por `AuthApplicationService`.

Autorização no estado atual do código:

- protegidos com `[Authorize]`: `ClientesController`, `VeiculosController`, `PecasController`;
- sem `[Authorize]`: `ServicosController`, `OrdensDeServicoController`, `AuthController`.

Esse comportamento deve ser entendido como estado atual da implementação, não necessariamente como política final de segurança.

## Observabilidade

Health checks expostos:

- `/health`
- `/health/live`
- `/health/ready`

O endpoint `/health` retorna um JSON com o estado geral da aplicação e do `DbContext`.

## Testabilidade

O repositório contém:

- testes unitários para serviços de aplicação;
- testes de integração para controllers;
- suporte a banco em memória para cenários de teste.
  ├─> Gerar número da ordem
  ├─> Criar OrdemDeServico (status: Recebida)
  ├─> OrdemDeServicoRepository.CriarAsync()
  ├─> DbContext.SaveChangesAsync()
  └─> Retornar OrdemDeServicoDto
```

## Banco de Dados

### Tecnologia
PostgreSQL 16

### Tabelas Principais
- `clientes`
- `veiculos`
- `servicos`
- `pecas`
- `ordens_de_servico`
- `ordem_de_servico_servicos` (many-to-many)
- `ordem_de_servico_pecas` (many-to-many)

### Migrations
Gerenciadas com Entity Framework Core.

```bash
dotnet ef migrations add AddClienteEntity
dotnet ef database update
```

## Containerização

### Dockerfile
Utiliza multi-stage builds para otimizar tamanho da imagem:
1. **base** - ASP.NET Core runtime
2. **build** - SDK .NET para compilar
3. **publish** - Publicar binários
4. **final** - Imagem final com aplicação

### Docker Compose
Orquestra:
- **PostgreSQL** - Banco de dados
- **API** - Aplicação .NET

## Segurança

### Validação de Dados
- CPF/CNPJ validados
- Email validado
- Placa de veículo com regex

### CORS
Configurado para aceitar requisições de qualquer origem em desenvolvimento.

### Usuário Non-Root
Dockerfile usa usuário `appuser` por segurança.

## Performance

### Connection Pooling
Configurado no Npgsql para gerenciar conexões eficientemente.

### Health Checks
Monitoram status da aplicação e banco de dados.

### Logging
Estruturado com diferentes níveis por ambiente.

## Escalabilidade

A arquitetura é preparada para:

1. **Microsserviços** - Cada camada pode ser extraída
2. **Load Balancing** - Múltiplas instâncias da API
3. **Read Replicas** - PostgreSQL read replicas
4. **Cache** - Redis para cache de dados
5. **Message Queue** - RabbitMQ/Kafka para processamento assíncronico

## Estrutura de Pastas

```
src/
├── API/
│   └── Controllers/
│       ├── ClientesController.cs
│       ├── VeiculosController.cs
│       ├── ServicosController.cs
│       ├── PecasController.cs
│       └── OrdensDeServicoController.cs
├── Application/
│   ├── DTOs/
│   │   ├── ClienteDto.cs
│   │   ├── VeiculoDto.cs
│   │   ├── ServicoDto.cs
│   │   ├── PecaDto.cs
│   │   └── OrdemDeServicoDto.cs
│   └── Services/
│       ├── ClienteApplicationService.cs
│       ├── VeiculoApplicationService.cs
│       ├── ServicoApplicationService.cs
│       ├── PecaApplicationService.cs
│       └── OrdemDeServicoApplicationService.cs
├── Domain/
│   ├── Entities/
│   │   ├── Cliente.cs
│   │   ├── Veiculo.cs
│   │   ├── Servico.cs
│   │   ├── Peca.cs
│   │   └── OrdemDeServico.cs
│   └── Repositories/
│       ├── IClienteRepository.cs
│       ├── IVeiculoRepository.cs
│       ├── IServicoRepository.cs
│       ├── IPecaRepository.cs
│       └── IOrdemDeServicoRepository.cs
└── Infrastructure/
    ├── Data/
    │   └── OficinaDbContext.cs
    ├── Repositories/
    │   ├── ClienteRepository.cs
    │   ├── VeiculoRepository.cs
    │   ├── ServicoRepository.cs
    │   ├── PecaRepository.cs
    │   └── OrdemDeServicoRepository.cs
    └── HealthChecks/
        └── HealthCheckConfiguration.cs

tests/
└── Oficina.UnitTests/
    ├── Application/
    │   └── Services/
    ├── Domain/
    │   └── Entities/
    └── Infrastructure/
        └── Repositories/
```

## Evolução Futura

### Curto Prazo
- Autenticação JWT
- Validação FluentValidation
- Testes de integração
- Logging estruturado

### Médio Prazo
- API Gateway
- Autenticação avançada
- Relatórios e analytics
- Notificações por email

### Longo Prazo
- Microsserviços
- Cache distribuído
- Message Queue
- GraphQL API
