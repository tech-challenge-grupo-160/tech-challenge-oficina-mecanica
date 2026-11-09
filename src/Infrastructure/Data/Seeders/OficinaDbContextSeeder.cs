using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Security;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Seeders;

[ExcludeFromCodeCoverage]
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
                    Cliente.Criar("Vanessa Luna Duarte", Documento.Parse("476.548.668-01"), Telefone.Parse("15984608796"), Email.Parse("vanessa_luna_duarte@maissaude.adm.br"), DateTimeHelper.UTCBrazilNow()),
                    Cliente.Criar("Rafael Mateus Cesar Souza", Documento.Parse("093.678.498-93"), Telefone.Parse("15983042238"), Email.Parse("rafael-souza91@gilbertorodrigues.com"), DateTimeHelper.UTCBrazilNow()),
                    Cliente.Criar("Betina e Fernanda Contabil Ltda", Documento.Parse("60.617.051/0001-99"), Telefone.Parse("16985344781"), Email.Parse("ouvidoria@betinaefernandacontabilltda.com.br"), DateTimeHelper.UTCBrazilNow()),
                    Cliente.Criar("Vicente e Giovanni Advocacia Ltda", Documento.Parse("46.981.686/0001-40"), Telefone.Parse("11983202194"), Email.Parse("contato@vicenteegiovanniadvocacialtda.com.br"), DateTimeHelper.UTCBrazilNow())
                };

                await context.Clientes.AddRangeAsync(clientes);
                await context.SaveChangesAsync();
            }

            if (!await context.Veiculos.AnyAsync())
            {
                var clientes = await context.Clientes.OrderBy(c => c.Id).ToListAsync();
                var clienteVanessa = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCpf("476.548.668-01"));
                var clienteRafael = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCpf("093.678.498-93"));
                var clienteBetina = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCnpj("60.617.051/0001-99"));
                var clienteVicente = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCnpj("46.981.686/0001-40"));

                var veiculos = new[]
                {
                    Veiculo.Criar(PlacaVeiculo.Parse("ABC1234"), "Toyota", "Corolla", 2020, clienteVanessa.Id),
                    Veiculo.Criar(PlacaVeiculo.Parse("XYZ5678"), "Honda", "Civic", 2019, clienteRafael.Id),
                    Veiculo.Criar(PlacaVeiculo.Parse("DEF9101"), "Volkswagen", "Gol", 2021, clienteBetina.Id),
                    Veiculo.Criar(PlacaVeiculo.Parse("GHI2345"), "Fiat", "Strada", 2022, clienteVicente.Id)
                };

                await context.Veiculos.AddRangeAsync(veiculos);
                await context.SaveChangesAsync();
            }

            if (!await context.OrdensDeServico.AnyAsync())
            {
                var agora = DateTimeHelper.UTCBrazilNow();
                var clientes = await context.Clientes.OrderBy(c => c.Id).ToListAsync();
                var veiculos = await context.Veiculos.OrderBy(v => v.Id).ToListAsync();
                var clienteVanessa = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCpf("476.548.668-01"));
                var clienteRafael = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCpf("093.678.498-93"));
                var clienteBetina = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCnpj("60.617.051/0001-99"));
                var clienteVicente = clientes.Single(c => c.CpfCnpj.Valor == DocumentoHelper.NormalizarCnpj("46.981.686/0001-40"));
                var veiculoCorolla = veiculos.Single(v => v.Placa.Valor == "ABC1234");
                var veiculoCivic = veiculos.Single(v => v.Placa.Valor == "XYZ5678");
                var veiculoGol = veiculos.Single(v => v.Placa.Valor == "DEF9101");
                var veiculoStrada = veiculos.Single(v => v.Placa.Valor == "GHI2345");
                var ordem1 = OrdemDeServico.Restaurar(
                    "OS-20260413-3000",
                    "AC-SEED-3000",
                    StringHelper.ToSha256Hash("seed-token-os-3000"),
                    clienteVanessa.Id,
                    veiculoCorolla.Id,
                    "Cliente relatou troca de oleo e revisao preventiva.",
                    "Veiculo recebido sem avarias aparentes.",
                    null,
                    StatusOrdemDeServico.Entregue,
                    agora.AddDays(-5),
                    agora.AddDays(-4).AddHours(2),
                    agora.AddDays(-2),
                    agora.AddDays(-2).AddHours(4),
                    agora.AddDays(-1),
                    195.00m);

                var ordem2 = OrdemDeServico.Restaurar(
                    "OS-20260416-3001",
                    "AC-SEED-3001",
                    StringHelper.ToSha256Hash("seed-token-os-3001"),
                    clienteRafael.Id,
                    veiculoCivic.Id,
                    "Cliente informou ruido na suspensao dianteira.",
                    "Barulho identificado em baixa velocidade.",
                    null,
                    StatusOrdemDeServico.Finalizada,
                    agora.AddDays(-2),
                    agora.AddDays(-1).AddHours(-18),
                    agora.AddHours(-8),
                    null,
                    null,
                    975.00m);

                var ordem3 = OrdemDeServico.Restaurar(
                    "OS-20260420-3002",
                    "AC-SEED-3002",
                    StringHelper.ToSha256Hash("seed-token-os-3002"),
                    clienteBetina.Id,
                    veiculoGol.Id,
                    "Veiculo com falha de ignicao intermitente.",
                    "Motor falhando apos aquecimento.",
                    null,
                    StatusOrdemDeServico.EmExecucao,
                    agora.AddDays(-1).AddHours(-6),
                    agora.AddDays(-1).AddHours(-1),
                    null,
                    null,
                    null,
                    650.00m);

                var ordem4 = OrdemDeServico.Restaurar(
                    "OS-20260421-3003",
                    "AC-SEED-3003",
                    StringHelper.ToSha256Hash("seed-token-os-3003"),
                    clienteVanessa.Id,
                    veiculoCorolla.Id,
                    "Luz da injecao acesa no painel.",
                    "Cliente autorizou apenas diagnostico inicial.",
                    null,
                    StatusOrdemDeServico.AguardandoAprovacao,
                    agora.AddHours(-18),
                    agora.AddHours(-6),
                    null,
                    null,
                    null,
                    100.00m);

                var ordem5 = OrdemDeServico.Restaurar(
                    "OS-20260421-3004",
                    "AC-SEED-3004",
                    StringHelper.ToSha256Hash("seed-token-os-3004"),
                    clienteRafael.Id,
                    veiculoCivic.Id,
                    "Revisao de freios e alinhamento.",
                    "Veiculo puxando para a direita.",
                    null,
                    StatusOrdemDeServico.EmDiagnostico,
                    agora.AddHours(-10),
                    null,
                    null,
                    null,
                    null,
                    300.00m);

                var ordem6 = OrdemDeServico.Restaurar(
                    "OS-20260422-3005",
                    "AC-SEED-3005",
                    StringHelper.ToSha256Hash("seed-token-os-3005"),
                    clienteVicente.Id,
                    veiculoStrada.Id,
                    "Veiculo recebido para revisao basica.",
                    "Aguardando entrada na oficina.",
                    null,
                    StatusOrdemDeServico.Recebida,
                    agora.AddHours(-3),
                    null,
                    null,
                    null,
                    null,
                    0.00m);

                var ordem7 = OrdemDeServico.Restaurar(
                    "OS-20260419-3006",
                    "AC-SEED-3006",
                    StringHelper.ToSha256Hash("seed-token-os-3006"),
                    clienteBetina.Id,
                    veiculoGol.Id,
                    "Solicitada avaliacao de pneus e suspensao.",
                    "Cliente desistiu antes da aprovacao do orcamento.",
                    "Cliente optou por nao prosseguir com o reparo.",
                    StatusOrdemDeServico.Cancelada,
                    agora.AddDays(-3),
                    null,
                    null,
                    null,
                    null,
                    0.00m);

                await context.OrdensDeServico.AddRangeAsync(ordem1, ordem2, ordem3, ordem4, ordem5, ordem6, ordem7);
                await context.SaveChangesAsync();

                var ordensServicos = new List<OrdemDeServicoServico>
                {
                    OrdemDeServicoServico.Criar(ordem1.Id, 1000, 150.00m, 30),
                    OrdemDeServicoServico.Criar(ordem2.Id, 1001, 500.00m, 180),
                    OrdemDeServicoServico.Criar(ordem2.Id, 1002, 200.00m, 60),
                    OrdemDeServicoServico.Criar(ordem3.Id, 1003, 300.00m, 90),
                    OrdemDeServicoServico.Criar(ordem3.Id, 1004, 100.00m, 45),
                    OrdemDeServicoServico.Criar(ordem4.Id, 1004, 100.00m, 45),
                    OrdemDeServicoServico.Criar(ordem5.Id, 1002, 200.00m, 60)
                };

                var ordensPecas = new List<OrdemDeServicoPeca>
                {
                    OrdemDeServicoPeca.Criar(ordem1.Id, 1000, 1, 45.00m),
                    OrdemDeServicoPeca.Criar(ordem2.Id, 1001, 1, 35.00m),
                    OrdemDeServicoPeca.Criar(ordem2.Id, 1002, 2, 120.00m),
                    OrdemDeServicoPeca.Criar(ordem3.Id, 1003, 1, 250.00m),
                    OrdemDeServicoPeca.Criar(ordem5.Id, 1004, 4, 25.00m)
                };

                await context.OrdemDeServicoServicos.AddRangeAsync(ordensServicos);
                await context.OrdemDeServicoPecas.AddRangeAsync(ordensPecas);
                await context.SaveChangesAsync();
            }

            if (!await context.Usuarios.AnyAsync())
            {
                var passwordHasher = new BCryptPasswordHasher();
                var usuario = Usuario.Criar(
                    "Administrador",
                    "admin",
                    passwordHasher.Hash("admin123"),
                    "Administrador");

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
