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
                var clienteVicente = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCnpj("46.981.686/0001-40"));

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
                    },
                    new Veiculo
                    {
                        Placa = "GHI2345",
                        Marca = "Fiat",
                        Modelo = "Strada",
                        Ano = 2022,
                        ClienteId = clienteVicente.Id
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
                var agora = DateTimeHelper.UTCBrazilNow();
                var clientes = await context.Clientes.OrderBy(c => c.Id).ToListAsync();
                var veiculos = await context.Veiculos.OrderBy(v => v.Id).ToListAsync();
                var servicos = await context.Servicos.OrderBy(s => s.Id).ToListAsync();
                var pecas = await context.Pecas.OrderBy(p => p.Id).ToListAsync();
                var clienteVanessa = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCpf("476.548.668-01"));
                var clienteRafael = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCpf("093.678.498-93"));
                var clienteBetina = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCnpj("60.617.051/0001-99"));
                var clienteVicente = clientes.Single(c => c.CpfCnpj == DocumentoHelper.NormalizarCnpj("46.981.686/0001-40"));
                var veiculoCorolla = veiculos.Single(v => v.Placa == "ABC1234");
                var veiculoCivic = veiculos.Single(v => v.Placa == "XYZ5678");
                var veiculoGol = veiculos.Single(v => v.Placa == "DEF9101");
                var veiculoStrada = veiculos.Single(v => v.Placa == "GHI2345");
                var servicoTrocaOleo = servicos.Single(s => s.Nome == "Troca de Oleo");
                var servicoRevisaoCompleta = servicos.Single(s => s.Nome == "Revisao Completa");
                var servicoAlinhamento = servicos.Single(s => s.Nome == "Alinhamento");
                var servicoTrocaPneus = servicos.Single(s => s.Nome == "Troca de Pneus");
                var servicoDiagnosticoEletronico = servicos.Single(s => s.Nome == "Diagnostico Eletronico");
                var pecaFiltroOleo = pecas.Single(p => p.Nome == "Filtro de Oleo");
                var pecaFiltroAr = pecas.Single(p => p.Nome == "Filtro de Ar");
                var pecaPastilhaFreio = pecas.Single(p => p.Nome == "Pastilha de Freio");
                var pecaPneuAro15 = pecas.Single(p => p.Nome == "Pneu Aro 15");
                var pecaVelaIgnicao = pecas.Single(p => p.Nome == "Vela de Ignicao");

                var ordem1 = new OrdemDeServico
                {
                    Numero = "OS-20260413-3000",
                    ClienteId = clienteVanessa.Id,
                    VeiculoId = veiculoCorolla.Id,
                    DescricaoSolicitacao = "Cliente relatou troca de oleo e revisao preventiva.",
                    ObservacoesRecepcao = "Veiculo recebido sem avarias aparentes.",
                    Status = StatusOrdemDeServico.Entregue,
                    DataAbertura = agora.AddDays(-5),
                    OrcamentoEnviadoEm = agora.AddDays(-4).AddHours(2),
                    DataFinalizacao = agora.AddDays(-2),
                    DataPagamento = agora.AddDays(-2).AddHours(4),
                    DataConclusao = agora.AddDays(-1),
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
                    DataAbertura = agora.AddDays(-2),
                    OrcamentoEnviadoEm = agora.AddDays(-1).AddHours(-18),
                    DataFinalizacao = agora.AddHours(-8),
                    DataConclusao = null,
                    ValorTotal = 975.00m
                };

                var ordem3 = new OrdemDeServico
                {
                    Numero = "OS-20260420-3002",
                    ClienteId = clienteBetina.Id,
                    VeiculoId = veiculoGol.Id,
                    DescricaoSolicitacao = "Veiculo com falha de ignicao intermitente.",
                    ObservacoesRecepcao = "Motor falhando apos aquecimento.",
                    Status = StatusOrdemDeServico.EmExecucao,
                    DataAbertura = agora.AddDays(-1).AddHours(-6),
                    OrcamentoEnviadoEm = agora.AddDays(-1).AddHours(-1),
                    ValorTotal = 650.00m
                };

                var ordem4 = new OrdemDeServico
                {
                    Numero = "OS-20260421-3003",
                    ClienteId = clienteVanessa.Id,
                    VeiculoId = veiculoCorolla.Id,
                    DescricaoSolicitacao = "Luz da injecao acesa no painel.",
                    ObservacoesRecepcao = "Cliente autorizou apenas diagnostico inicial.",
                    Status = StatusOrdemDeServico.AguardandoAprovacao,
                    DataAbertura = agora.AddHours(-18),
                    OrcamentoEnviadoEm = agora.AddHours(-6),
                    ValorTotal = 100.00m
                };

                var ordem5 = new OrdemDeServico
                {
                    Numero = "OS-20260421-3004",
                    ClienteId = clienteRafael.Id,
                    VeiculoId = veiculoCivic.Id,
                    DescricaoSolicitacao = "Revisao de freios e alinhamento.",
                    ObservacoesRecepcao = "Veiculo puxando para a direita.",
                    Status = StatusOrdemDeServico.EmDiagnostico,
                    DataAbertura = agora.AddHours(-10),
                    ValorTotal = 300.00m
                };

                var ordem6 = new OrdemDeServico
                {
                    Numero = "OS-20260422-3005",
                    ClienteId = clienteVicente.Id,
                    VeiculoId = veiculoStrada.Id,
                    DescricaoSolicitacao = "Veiculo recebido para revisao basica.",
                    ObservacoesRecepcao = "Aguardando entrada na oficina.",
                    Status = StatusOrdemDeServico.Recebida,
                    DataAbertura = agora.AddHours(-3),
                    ValorTotal = 0.00m
                };

                var ordem7 = new OrdemDeServico
                {
                    Numero = "OS-20260419-3006",
                    ClienteId = clienteBetina.Id,
                    VeiculoId = veiculoGol.Id,
                    DescricaoSolicitacao = "Solicitada avaliacao de pneus e suspensao.",
                    ObservacoesRecepcao = "Cliente desistiu antes da aprovacao do orcamento.",
                    MotivoCancelamento = "Cliente optou por nao prosseguir com o reparo.",
                    Status = StatusOrdemDeServico.Cancelada,
                    DataAbertura = agora.AddDays(-3),
                    ValorTotal = 0.00m
                };

                await context.OrdensDeServico.AddRangeAsync(ordem1, ordem2, ordem3, ordem4, ordem5, ordem6, ordem7);
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
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem3.Id,
                        ServicoId = servicoTrocaPneus.Id,
                        Preco = servicoTrocaPneus.Preco,
                        TempoEstimado = servicoTrocaPneus.TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem3.Id,
                        ServicoId = servicoDiagnosticoEletronico.Id,
                        Preco = servicoDiagnosticoEletronico.Preco,
                        TempoEstimado = servicoDiagnosticoEletronico.TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem4.Id,
                        ServicoId = servicoDiagnosticoEletronico.Id,
                        Preco = servicoDiagnosticoEletronico.Preco,
                        TempoEstimado = servicoDiagnosticoEletronico.TempoEstimado
                    },
                    new OrdemDeServicoServico
                    {
                        OrdemDeServicoId = ordem5.Id,
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
                    },
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordem3.Id,
                        PecaId = pecaPneuAro15.Id,
                        Quantidade = 1,
                        Preco = pecaPneuAro15.Preco
                    },
                    new OrdemDeServicoPeca
                    {
                        OrdemDeServicoId = ordem5.Id,
                        PecaId = pecaVelaIgnicao.Id,
                        Quantidade = 4,
                        Preco = pecaVelaIgnicao.Preco
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
