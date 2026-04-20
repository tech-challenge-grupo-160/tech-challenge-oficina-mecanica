using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Seeders;

public static class OficinaDbContextSeeder
{
    public static async Task SeedAsync(OficinaDbContext context)
    {
        try
        {
            if (!await context.Clientes.AnyAsync())
            {
                var clientes = new[]
                {
                    new Cliente
                    {
                        Nome = "Vanessa Luna Duarte",
                        CpfCnpj = DocumentoHelper.NormalizarCpf("476.548.668-01"),
                        Telefone = StringHelper.OnlyDigits("15984608796"),
                        Email = "vanessa_luna_duarte@maissaude.adm.br",
                        DataCadastro = DateTimeHelper.UTCBrazilNow()
                    },
                    new Cliente
                    {
                        Nome = "Rafael Mateus Cesar Souza",
                        CpfCnpj = DocumentoHelper.NormalizarCpf("093.678.498-93"),
                        Telefone = StringHelper.OnlyDigits("15983042238"),
                        Email = "rafael-souza91@gilbertorodrigues.com",
                        DataCadastro = DateTimeHelper.UTCBrazilNow()
                    },
                    new Cliente
                    {
                        Nome = "Betina e Fernanda Contabil Ltda",
                        CpfCnpj = DocumentoHelper.NormalizarCnpj("60.617.051/0001-99"),
                        Telefone = StringHelper.OnlyDigits("16985344781"),
                        Email = "ouvidoria@betinaefernandacontabilltda.com.br",
                        DataCadastro = DateTimeHelper.UTCBrazilNow()
                    },
                    new Cliente
                    {
                        Nome = "Vicente e Giovanni Advocacia Ltda",
                        CpfCnpj = DocumentoHelper.NormalizarCnpj("46.981.686/0001-40"),
                        Telefone = StringHelper.OnlyDigits("11983202194"),
                        Email = "contato@vicenteegiovanniadvocacialtda.com.br",
                        DataCadastro = DateTimeHelper.UTCBrazilNow()
                    }
                };

                await context.Clientes.AddRangeAsync(clientes);
                await context.SaveChangesAsync();
            }

            if (!await context.Veiculos.AnyAsync())
            {
                var clientes = await context.Clientes.OrderBy(c => c.Id).ToListAsync();
                var clienteVanessa = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCpf("476.548.668-01"));
                var clienteRafael = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCpf("093.678.498-93"));
                var clienteBetina = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCnpj("60.617.051/0001-99"));

                var veiculos = new[]
                {
                    new Veiculo
                    {
                        Placa = "ABC1234",
                        Marca = "Toyota",
                        Modelo = "Corolla",
                        Ano = 2020,
                        ClienteId = clienteVanessa.Id
                    },
                    new Veiculo
                    {
                        Placa = "XYZ5678",
                        Marca = "Honda",
                        Modelo = "Civic",
                        Ano = 2019,
                        ClienteId = clienteRafael.Id
                    },
                    new Veiculo
                    {
                        Placa = "DEF9101",
                        Marca = "Volkswagen",
                        Modelo = "Gol",
                        Ano = 2021,
                        ClienteId = clienteBetina.Id
                    }
                };

                await context.Veiculos.AddRangeAsync(veiculos);
                await context.SaveChangesAsync();
            }

            if (!await context.Servicos.AnyAsync())
            {
                var servicos = new[]
                {
                    new Servico
                    {
                        Nome = "Troca de Oleo",
                        Descricao = "Troca de oleo do motor e filtro",
                        Preco = 150.00m,
                        TempoEstimado = 30
                    },
                    new Servico
                    {
                        Nome = "Revisao Completa",
                        Descricao = "Revisao completa do veiculo",
                        Preco = 500.00m,
                        TempoEstimado = 180
                    },
                    new Servico
                    {
                        Nome = "Alinhamento",
                        Descricao = "Alinhamento e balanceamento",
                        Preco = 200.00m,
                        TempoEstimado = 60
                    },
                    new Servico
                    {
                        Nome = "Troca de Pneus",
                        Descricao = "Troca de pneus do veiculo",
                        Preco = 300.00m,
                        TempoEstimado = 90
                    },
                    new Servico
                    {
                        Nome = "Diagnostico Eletronico",
                        Descricao = "Diagnostico eletronico do motor",
                        Preco = 100.00m,
                        TempoEstimado = 45
                    }
                };

                await context.Servicos.AddRangeAsync(servicos);
                await context.SaveChangesAsync();
            }

            if (!await context.Pecas.AnyAsync())
            {
                var pecas = new[]
                {
                    new Peca { Nome = "Filtro de Oleo", Preco = 45.00m, QuantidadeEstoque = 50 },
                    new Peca { Nome = "Filtro de Ar", Preco = 35.00m, QuantidadeEstoque = 40 },
                    new Peca { Nome = "Pastilha de Freio", Preco = 120.00m, QuantidadeEstoque = 30 },
                    new Peca { Nome = "Pneu Aro 15", Preco = 250.00m, QuantidadeEstoque = 20 },
                    new Peca { Nome = "Vela de Ignicao", Preco = 25.00m, QuantidadeEstoque = 100 }
                };

                await context.Pecas.AddRangeAsync(pecas);
                await context.SaveChangesAsync();
            }

            if (!await context.OrdensDeServico.AnyAsync())
            {
                var clientes = await context.Clientes.OrderBy(c => c.Id).ToListAsync();
                var veiculos = await context.Veiculos.OrderBy(v => v.Id).ToListAsync();
                var servicos = await context.Servicos.OrderBy(s => s.Id).ToListAsync();
                var pecas = await context.Pecas.OrderBy(p => p.Id).ToListAsync();
                var clienteVanessa = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCpf("476.548.668-01"));
                var clienteRafael = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCpf("093.678.498-93"));
                var veiculoCorolla = veiculos.Single(v => v.Placa == "ABC1234");
                var veiculoCivic = veiculos.Single(v => v.Placa == "XYZ5678");
                var servicoTrocaOleo = servicos.Single(s => s.Nome == "Troca de Oleo");
                var servicoRevisaoCompleta = servicos.Single(s => s.Nome == "Revisao Completa");
                var servicoAlinhamento = servicos.Single(s => s.Nome == "Alinhamento");
                var pecaFiltroOleo = pecas.Single(p => p.Nome == "Filtro de Oleo");
                var pecaFiltroAr = pecas.Single(p => p.Nome == "Filtro de Ar");
                var pecaPastilhaFreio = pecas.Single(p => p.Nome == "Pastilha de Freio");

                var ordem1 = new OrdemDeServico
                {
                    Numero = "OS-20260413-3000",
                    ClienteId = clienteVanessa.Id,
                    VeiculoId = veiculoCorolla.Id,
                    DescricaoSolicitacao = "Cliente relatou troca de oleo e revisao preventiva.",
                    ObservacoesRecepcao = "Veiculo recebido sem avarias aparentes.",
                    Status = StatusOrdemDeServico.Entregue,
                    DataAbertura = DateTimeHelper.UTCBrazilNow().AddDays(-5),
                    DataConclusao = DateTimeHelper.UTCBrazilNow().AddDays(-1),
                    ValorTotal = 195.00m
                };

                var ordem2 = new OrdemDeServico
                {
                    Numero = "OS-20260416-3001",
                    ClienteId = clienteRafael.Id,
                    VeiculoId = veiculoCivic.Id,
                    DescricaoSolicitacao = "Cliente informou ruido na suspensao dianteira.",
                    ObservacoesRecepcao = "Barulho identificado em baixa velocidade.",
                    Status = StatusOrdemDeServico.Finalizada,
                    DataAbertura = DateTimeHelper.UTCBrazilNow().AddDays(-2),
                    DataConclusao = null,
                    ValorTotal = 700.00m
                };

                await context.OrdensDeServico.AddRangeAsync(ordem1, ordem2);
                await context.SaveChangesAsync();

                var ordensServicos = new List<OrdemDeServicoServico>
                {
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem1.Id,
                        ServicoId = servicoTrocaOleo.Id,
                        Preco = servicoTrocaOleo.Preco,
                        TempoEstimado = servicoTrocaOleo.TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem2.Id,
                        ServicoId = servicoRevisaoCompleta.Id,
                        Preco = servicoRevisaoCompleta.Preco,
                        TempoEstimado = servicoRevisaoCompleta.TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem2.Id,
                        ServicoId = servicoAlinhamento.Id,
                        Preco = servicoAlinhamento.Preco,
                        TempoEstimado = servicoAlinhamento.TempoEstimado
                    }
                };

                var ordensPecas = new List<OrdemDeServicoPeca>
                {
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordem1.Id,
                        PecaId = pecaFiltroOleo.Id,
                        Quantidade = 1,
                        Preco = pecaFiltroOleo.Preco
                    },
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordem2.Id,
                        PecaId = pecaFiltroAr.Id,
                        Quantidade = 1,
                        Preco = pecaFiltroAr.Preco
                    },
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordem2.Id,
                        PecaId = pecaPastilhaFreio.Id,
                        Quantidade = 2,
                        Preco = pecaPastilhaFreio.Preco
                    }
                };

                await context.OrdemDeServicoServicos.AddRangeAsync(ordensServicos);
                await context.OrdemDeServicoPecas.AddRangeAsync(ordensPecas);
                await context.SaveChangesAsync();
            }

            if (!await context.Usuarios.AnyAsync())
            {
                var usuario = new Usuario
                {
                    Nome = "Administrador",
                    UsuarioLogin = "admin",
                    Role = "Administrador",
                    SenhaHash = StringHelper.ToMd5Hash("admin123")
                };

                await context.Usuarios.AddAsync(usuario);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao fazer seed do banco de dados", ex);
        }
    }
}
