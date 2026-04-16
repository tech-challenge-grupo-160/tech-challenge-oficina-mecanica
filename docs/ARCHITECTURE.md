# Arquitetura - Sistema de Gestão de Oficina Mecânica

## Visão Geral

O sistema é uma aplicação monolítica em camadas, desenvolvida em C# com .NET 8, seguindo os princípios de **Domain-Driven Design (DDD)** e arquitetura em camadas.

## Arquitetura em Camadas

```
┌─────────────────────────────────────┐
│      API Layer (Presentation)       │
│  ├─ Controllers                      │
│  ├─ Error Handling                   │
│  └─ Request/Response Mapping         │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Application Layer (Use Cases)      │
│  ├─ Application Services             │
│  ├─ DTOs (Data Transfer Objects)     │
│  └─ Interfaces de Repositórios       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│    Domain Layer (Business Rules)     │
│  ├─ Entidades                        │
│  ├─ Value Objects                    │
│  ├─ Agregados                        │
│  └─ Interfaces de Repositórios       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│ Infrastructure Layer (Data Access)  │
│  ├─ DbContext (Entity Framework)     │
│  ├─ Repositórios (implementações)    │
│  ├─ Health Checks                    │
│  └─ Integrações Externas             │
└─────────────────────────────────────┘
```

## Fluxo de Requisição

```
1. HTTP Request
   └─> ApiController
       └─> IApplicationService
           └─> Domain Logic / Validation
               └─> IRepository (Interface)
                   └─> Repository Implementation
                       └─> DbContext
                           └─> Database
```

## Camada de Apresentação (API)

**Localização:** `src/API/Controllers/`

Responsabilidades:
- Receber requisições HTTP
- Validar dados de entrada
- Mapear DTOs para entidades
- Retornar respostas HTTP
- Tratamento de exceções

**Controllers:**
- `ClientesController`
- `VeiculosController`
- `ServicosController`
- `PecasController`
- `OrdensDeServicoController`

## Camada de Aplicação

**Localização:** `src/Application/`

### Application Services

Implementam os casos de uso do sistema:

```csharp
public interface IClienteApplicationService
{
    Task<ClienteDto> CriarClienteAsync(CriarClienteDto dto);
    Task<ClienteDto> ObterClienteAsync(int id);
    Task<IEnumerable<ClienteDto>> ListarClientesAsync();
    Task<ClienteDto> AtualizarClienteAsync(int id, AtualizarClienteDto dto);
    Task DeletarClienteAsync(int id);
}
```

### DTOs (Data Transfer Objects)

Separam a representação de dados da API da lógica interna:

```csharp
public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    // ...
}
```

## Camada de Domínio

**Localização:** `src/Domain/`

### Entidades

Representam conceitos principais do negócio:

- **Cliente** - Clientes da oficina
- **Veiculo** - Veículos dos clientes
- **Servico** - Serviços oferecidos
- **Peca** - Peças e insumos
- **OrdemDeServico** - Agregado raiz

### Agregados

`OrdemDeServico` é um agregado que contém:
- Referências a Cliente e Veiculo
- Coleção de Servicos
- Coleção de Pecas
- Lógica de negócio

### Interfaces de Repositório

Definem o contrato para acesso a dados:

```csharp
public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj);
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    Task<Cliente> CriarAsync(Cliente cliente);
    Task<Cliente> AtualizarAsync(Cliente cliente);
    Task DeletarAsync(int id);
}
```

## Camada de Infraestrutura

**Localização:** `src/Infrastructure/`

### DbContext

```csharp
public class OficinaDbContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    // ...
}
```

### Repositórios

Implementam as interfaces definidas no domínio:

```csharp
public class ClienteRepository : IClienteRepository
{
    private readonly OficinaDbContext _context;
    
    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }
}
```

### Health Checks

Monitoram a saúde da aplicação:

- `/health` - Status geral
- `/health/live` - Liveness (aplicação está rodando)
- `/health/ready` - Readiness (pronto para receber requisições)

## Padrões de Design Utilizados

### 1. Repository Pattern
Abstração para acesso a dados, permitindo trocar a implementação facilmente.

### 2. Dependency Injection
Inversão de controle para gerenciar dependências.

### 3. DTO (Data Transfer Object)
Separação entre a API e a lógica interna.

### 4. Async/Await
Operações assíncronas para melhor performance.

### 5. Aggregate Root
OrdemDeServico como raiz do agregado.

## Fluxo de Dados

### Criação de Cliente

```
POST /api/clientes
  ├─> ClientesController.Criar(CriarClienteDto)
  ├─> IClienteApplicationService.CriarClienteAsync(dto)
  ├─> Validar CPF/CNPJ
  ├─> Criar entidade Cliente
  ├─> IClienteRepository.CriarAsync(cliente)
  ├─> DbContext.SaveChangesAsync()
  └─> Retornar ClienteDto
```

### Criar Ordem de Serviço

```
POST /api/ordens-servico
  ├─> OrdensDeServicoController.Criar(CriarOrdemDeServicoDto)
  ├─> Validar cliente e veículo
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
