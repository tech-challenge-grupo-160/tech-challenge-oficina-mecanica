using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

public class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Veiculo> Veiculos { get; set; } = null!;
    public DbSet<Servico> Servicos { get; set; } = null!;
    public DbSet<Peca> Pecas { get; set; } = null!;
    public DbSet<OrdemDeServico> OrdensDeServico { get; set; } = null!;
    public DbSet<OrdemDeServicoServico> OrdemDeServicoServicos { get; set; } = null!;
    public DbSet<OrdemDeServicoPeca> OrdemDeServicoPecas { get; set; } = null!;
    public DbSet<OrdemServicoHistorico> OrdemServicoHistoricos { get; set; } = null!;
    public DbSet<NotificacaoCliente> NotificacoesCliente { get; set; } = null!;
    public DbSet<PedidoCompra> PedidosCompra { get; set; } = null!;
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OficinaDbContext).Assembly);
    }
}
