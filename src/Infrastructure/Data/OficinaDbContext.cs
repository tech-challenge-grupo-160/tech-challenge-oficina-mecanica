using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Observability;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

public class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Veiculo> Veiculos { get; set; } = null!;
    public DbSet<OrdemDeServico> OrdensDeServico { get; set; } = null!;
    public DbSet<OrdemDeServicoServico> OrdemDeServicoServicos { get; set; } = null!;
    public DbSet<OrdemDeServicoPeca> OrdemDeServicoPecas { get; set; } = null!;
    public DbSet<OrdemServicoHistorico> OrdemServicoHistoricos { get; set; } = null!;
    public DbSet<NotificacaoCliente> NotificacoesCliente { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;
    public DbSet<SagaInicioDiagnostico> SagasInicioDiagnostico { get; set; } = null!;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RegistrarTraceContext();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        RegistrarTraceContext();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Grava o contexto do trace ativo nas mensagens novas da Outbox e da Inbox,
    /// sem cada ponto que cria mensagem precisar lembrar disso (F4-31). A Outbox
    /// usa o contexto para a publicacao continuar o trace de quem gerou a
    /// mensagem; a Inbox guarda o trace da Saga para etapas humanas.
    /// </summary>
    private void RegistrarTraceContext()
    {
        string? contexto = null;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            switch (entry.Entity)
            {
                case OutboxMessage { TraceContext: null } outbox:
                    outbox.RegistrarTraceContext(contexto ??= MensageriaTrace.CapturarContextoAtual());
                    break;
                case InboxMessage { TraceContext: null } inbox:
                    inbox.RegistrarTraceContext(contexto ??= MensageriaTrace.CapturarContextoAtual());
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OficinaDbContext).Assembly);
    }
}
