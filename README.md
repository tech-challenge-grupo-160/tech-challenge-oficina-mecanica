# Sistema Integrado de Gestão de Oficina Mecânica

## Documentação Técnica – MVP Back-end em C#

---

# 1. Visão Geral

O sistema tem como objetivo fornecer uma plataforma para gestão de **ordens de serviço (OS)** em uma oficina mecânica, permitindo controlar clientes, veículos, serviços, peças e acompanhar o status dos atendimentos.

A solução proposta consiste em um **back-end monolítico desenvolvido em C# utilizando .NET**, aplicando princípios de **Domain Driven Design (DDD)** e boas práticas de arquitetura e segurança.

### Principais funcionalidades

* Criação e gestão de ordens de serviço
* Controle de estoque de peças e insumos
* Cadastro de clientes e veículos
* Acompanhamento do status da manutenção
* Consulta do progresso da OS via API

---

# 2. Arquitetura da Solução

A aplicação segue o modelo de **Monólito Modular com Arquitetura em Camadas**.

```
API (Presentation Layer)
Application Layer
Domain Layer
Infrastructure Layer
```

---

# 3. Camadas da Aplicação

## 3.1 API Layer (Presentation)

Responsável pela exposição das **APIs REST**.

### Tecnologias

* ASP.NET Core Web API
* Swagger / OpenAPI

### Responsabilidades

* Receber requisições HTTP
* Validar dados de entrada
* Autenticar usuários via JWT
* Encaminhar chamadas para a Application Layer

---

## 3.2 Application Layer

Responsável por implementar os **casos de uso do sistema**.

### Exemplos de casos de uso

* Criar Ordem de Serviço
* Aprovar orçamento
* Atualizar status da OS
* Registrar peças utilizadas

### Componentes

* Application Services
* DTOs
* Interfaces de repositórios

---

## 3.3 Domain Layer

Camada central do sistema contendo as **regras de negócio**.

### Componentes

* Entidades
* Value Objects
* Agregados
* Regras de domínio
* Interfaces de domínio

---

## 3.4 Infrastructure Layer

Responsável pela comunicação com recursos externos.

### Responsabilidades

* Persistência de dados
* Implementação de repositórios
* Configuração do ORM
* Autenticação e segurança
* Logs e integrações externas

### Tecnologias

* Entity Framework Core
* PostgreSQL

---

# 4. Modelagem de Domínio (DDD)

## Entidades Principais

### Cliente

Representa o cliente da oficina.

```csharp
public class Cliente
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string CpfCnpj { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public DateTime DataCadastro { get; set; }
}
```

---

### Veiculo

Representa o veículo associado ao cliente.

```csharp
public class Veiculo
{
    public Guid Id { get; set; }
    public string Placa { get; set; }
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public int Ano { get; set; }
    public Guid ClienteId { get; set; }
}
```

---

### OrdemDeServico

Agregado principal do sistema.

```csharp
public class OrdemDeServico
{
    public Guid Id { get; set; }
    public string Numero { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string Status { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataConclusao { get; set; }
    public decimal ValorTotal { get; set; }
}
```

---

### Servico

Representa os serviços oferecidos pela oficina.

```csharp
public class Servico
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}
```

---

### Peca

Representa peças e insumos utilizados nos serviços.

```csharp
public class Peca
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
```

---

# 5. Fluxo de Ordem de Serviço

## Criação da OS

Fluxo principal:

1. Identificação do cliente por CPF ou CNPJ
2. Cadastro ou seleção do veículo
3. Inclusão dos serviços solicitados
4. Inclusão de peças e insumos necessários
5. Cálculo automático do orçamento
6. Envio do orçamento para aprovação do cliente

---

## Status da Ordem de Serviço

Estados possíveis:

```
Recebida
EmDiagnostico
AguardandoAprovacao
EmExecucao
Finalizada
Entregue
```

### Fluxo de estados

```
Recebida → EmDiagnostico
EmDiagnostico → AguardandoAprovacao
AguardandoAprovacao → EmExecucao
EmExecucao → Finalizada
Finalizada → Entregue
```

---

# 6. APIs REST

## Clientes

```
POST   /api/clientes
GET    /api/clientes
GET    /api/clientes/{id}
PUT    /api/clientes/{id}
DELETE /api/clientes/{id}
```

---

## Veículos

```
POST   /api/veiculos
GET    /api/veiculos
GET    /api/veiculos/{id}
```

---

## Ordens de Serviço

```
POST   /api/ordens-servico
GET    /api/ordens-servico
GET    /api/ordens-servico/{id}
PUT    /api/ordens-servico/{id}/status
```

---

## Peças

```
POST   /api/pecas
GET    /api/pecas
PUT    /api/pecas/{id}
```

---

# 7. Segurança

## Autenticação

As APIs administrativas são protegidas utilizando **JWT (JSON Web Token)**.

### Fluxo de autenticação

1. Usuário realiza login
2. API gera um token JWT
3. O token é enviado nas requisições

```
Authorization: Bearer {token}
```

---

## Validação de Dados Sensíveis

### CPF / CNPJ

* Validação de formato
* Verificação de dígitos verificadores

### Placa de Veículo

Regex para padrão Mercosul:

```
[A-Z]{3}[0-9][A-Z][0-9]{2}
```

---

# 8. Banco de Dados

Banco escolhido: **PostgreSQL**

### Justificativa

* Open Source
* Alta confiabilidade
* Excelente suporte no .NET
* Forte suporte a transações

### ORM utilizado

* Entity Framework Core

---

# 9. Testes Automatizados

## Testes Unitários

Ferramentas:

* xUnit
* FluentAssertions

Cobertura em:

* Domínio
* Regras de negócio
* Application Services

---

## Testes de Integração

Validam:

* APIs
* Repositórios
* Integração com banco

Ferramentas:

* WebApplicationFactory
* TestContainers

### Meta de cobertura

**80% de cobertura nos domínios críticos**

---

# 10. Documentação da API

A documentação será gerada automaticamente utilizando **Swagger / OpenAPI**.

Endpoint:

```
/swagger
```

Permite:

* Testar endpoints
* Visualizar contratos da API
* Validar requisições

---

# 11. Containerização

## Dockerfile

Responsável por construir a imagem da aplicação.

### Etapas

1. Build da aplicação
2. Publicação
3. Execução dentro do container

---

## Docker Compose

Utilizado para orquestrar o ambiente.

### Serviços

```
api
postgres
```

### Inicialização

```
docker-compose up --build
```

---

# 12. Estrutura do Projeto

```
src/

Oficina.API
Oficina.Application
Oficina.Domain
Oficina.Infrastructure

tests/

Oficina.UnitTests
Oficina.IntegrationTests
```

---

# 13. Execução do Projeto

## Pré-requisitos

* Docker
* .NET 8 SDK

---

## Execução com Docker

```
docker-compose up --build
```

---

## Execução local

```
dotnet restore
dotnet build
dotnet run
```

---

# 14. Análise de Vulnerabilidades

Ferramentas recomendadas:

* Snyk
* OWASP Dependency Check
* SonarQube

### Itens analisados

* Dependências vulneráveis
* Segurança de autenticação
* Validação de dados
* Exposição de endpoints

---

# 15. Conclusão

A solução proposta atende aos requisitos do desafio ao fornecer:

* Arquitetura baseada em DDD
* Back-end monolítico em camadas
* APIs REST documentadas
* Autenticação segura com JWT
* Testes automatizados
* Ambiente containerizado com Docker

Este MVP estabelece uma base sólida para evolução futura do sistema, incluindo integrações com aplicativos móveis e dashboards administrativos.
