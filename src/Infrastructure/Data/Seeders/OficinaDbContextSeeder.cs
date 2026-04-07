using oficina_mecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace oficina_mecanica.Infrastructure.Data.Seeders;

public static class OficinaDbContextSeeder
{
    public static async Task SeedAsync(OficinaDbContext context)
    {
        try
        {
            // Seed Clientes
            if (!await context.Clientes.AnyAsync())
            {
                var clientes = new[]
                {
                    new Cliente
                    {
                        Id = Guid.NewGuid(),
                        Nome = "João Silva",
                        CpfCnpj = "123.456.789-10",
                        Telefone = "(11) 98765-4321",
                        Email = "joao.silva@email.com",
                        DataCadastro = DateTime.UtcNow
                    },
                    new Cliente
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Maria Santos",
                        CpfCnpj = "987.654.321-10",
                        Telefone = "(11) 97654-3210",
                        Email = "maria.santos@email.com",
                        DataCadastro = DateTime.UtcNow
                    },
                    new Cliente
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Transportadora XYZ LTDA",
                        CpfCnpj = "12.345.678/0001-90",
                        Telefone = "(11) 3456-7890",
                        Email = "contato@transportadora.com",
                        DataCadastro = DateTime.UtcNow
                    }
                };

                await context.Clientes.AddRangeAsync(clientes);
                await context.SaveChangesAsync();
            }

            // Seed Veículos
            if (!await context.Veiculos.AnyAsync())
            {
                var clientes = await context.Clientes.ToListAsync();
                var veiculos = new[]
                {
                    new Veiculo
                    {
                        Id = Guid.NewGuid(),
                        Placa = "ABC-1234",
                        Marca = "Toyota",
                        Modelo = "Corolla",
                        Ano = 2020,
                        ClienteId = clientes[0].Id
                    },
                    new Veiculo
                    {
                        Id = Guid.NewGuid(),
                        Placa = "XYZ-5678",
                        Marca = "Honda",
                        Modelo = "Civic",
                        Ano = 2019,
                        ClienteId = clientes[1].Id
                    },
                    new Veiculo
                    {
                        Id = Guid.NewGuid(),
                        Placa = "DEF-9101",
                        Marca = "Volkswagen",
                        Modelo = "Gol",
                        Ano = 2021,
                        ClienteId = clientes[2].Id
                    }
                };

                await context.Veiculos.AddRangeAsync(veiculos);
                await context.SaveChangesAsync();
            }

            // Seed Serviços
            if (!await context.Servicos.AnyAsync())
            {
                var servicos = new[]
                {
                    new Servico
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Troca de Óleo",
                        Descricao = "Troca de óleo do motor e filtro",
                        Preco = 150.00m,
                        TempoEstimado = 30
                    },
                    new Servico
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Revisão Completa",
                        Descricao = "Revisão completa do veículo",
                        Preco = 500.00m,
                        TempoEstimado = 180
                    },
                    new Servico
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Alinhamento",
                        Descricao = "Alinhamento e balanceamento",
                        Preco = 200.00m,
                        TempoEstimado = 60
                    },
                    new Servico
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Troca de Pneus",
                        Descricao = "Troca de pneus do veículo",
                        Preco = 300.00m,
                        TempoEstimado = 90
                    },
                    new Servico
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Diagnóstico Eletrônico",
                        Descricao = "Diagnóstico eletrônico do motor",
                        Preco = 100.00m,
                        TempoEstimado = 45
                    }
                };

                await context.Servicos.AddRangeAsync(servicos);
                await context.SaveChangesAsync();
            }

            // Seed Peças
            if (!await context.Pecas.AnyAsync())
            {
                var pecas = new[]
                {
                    new Peca
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Filtro de Óleo",
                        Preco = 45.00m,
                        QuantidadeEstoque = 50
                    },
                    new Peca
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Filtro de Ar",
                        Preco = 35.00m,
                        QuantidadeEstoque = 40
                    },
                    new Peca
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Pastilha de Freio",
                        Preco = 120.00m,
                        QuantidadeEstoque = 30
                    },
                    new Peca
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Pneu Aro 15",
                        Preco = 250.00m,
                        QuantidadeEstoque = 20
                    },
                    new Peca
                    {
                        Id = Guid.NewGuid(),
                        Nome = "Vela de Ignição",
                        Preco = 25.00m,
                        QuantidadeEstoque = 100
                    }
                };

                await context.Pecas.AddRangeAsync(pecas);
                await context.SaveChangesAsync();
            }

            // Seed Ordens de Serviço
            if (!await context.OrdensDeServico.AnyAsync())
            {
                var clientes = await context.Clientes.ToListAsync();
                var veiculos = await context.Veiculos.ToListAsync();
                var servicos = await context.Servicos.ToListAsync();
                var pecas = await context.Pecas.ToListAsync();

                var ordemId1 = Guid.NewGuid();
                var ordemId2 = Guid.NewGuid();

                var ordens = new[]
                {
                    new OrdemDeServico
                    {
                        Id = ordemId1,
                        Numero = "OS-001",
                        ClienteId = clientes[0].Id,
                        VeiculoId = veiculos[0].Id,
                        Status = StatusOrdemDeServico.Entregue,
                        DataAbertura = DateTime.UtcNow.AddDays(-5),
                        DataConclusao = DateTime.UtcNow.AddDays(-1),
                        ValorTotal = 195.00m
                    },
                    new OrdemDeServico
                    {
                        Id = ordemId2,
                        Numero = "OS-002",
                        ClienteId = clientes[1].Id,
                        VeiculoId = veiculos[1].Id,
                        Status = StatusOrdemDeServico.EmExecucao,
                        DataAbertura = DateTime.UtcNow.AddDays(-2),
                        DataConclusao = null,
                        ValorTotal = 700.00m
                    }
                };

                await context.OrdensDeServico.AddRangeAsync(ordens);
                await context.SaveChangesAsync();

                // Seed Ordem de Serviço - Serviços
                var ordensServicos = new List<OrdemDeServicoServico>
                {
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordemId1,
                        ServicoId = servicos[0].Id,
                        Preco = servicos[0].Preco,
                        TempoEstimado = servicos[0].TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordemId2,
                        ServicoId = servicos[1].Id,
                        Preco = servicos[1].Preco,
                        TempoEstimado = servicos[1].TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordemId2,
                        ServicoId = servicos[2].Id,
                        Preco = servicos[2].Preco,
                        TempoEstimado = servicos[2].TempoEstimado
                    }
                };

                await context.OrdemDeServicoServicos.AddRangeAsync(ordensServicos);

                // Seed Ordem de Serviço - Peças
                var ordensPecas = new List<OrdemDeServicoPeca>
                {
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordemId1,
                        PecaId = pecas[0].Id,
                        Quantidade = 1,
                        Preco = pecas[0].Preco
                    },
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordemId2,
                        PecaId = pecas[1].Id,
                        Quantidade = 1,
                        Preco = pecas[1].Preco
                    },
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordemId2,
                        PecaId = pecas[2].Id,
                        Quantidade = 2,
                        Preco = pecas[2].Preco
                    }
                };

                await context.OrdemDeServicoPecas.AddRangeAsync(ordensPecas);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao fazer seed do banco de dados", ex);
        }
    }
}
