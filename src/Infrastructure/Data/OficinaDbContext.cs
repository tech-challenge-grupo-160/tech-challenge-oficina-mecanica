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
    public DbSet<PedidoCompra> PedidosCompra { get; set; } = null!;
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1000);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CpfCnpj).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.CpfCnpj).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasMany(e => e.Veiculos).WithOne(v => v.Cliente).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.OrdensDeServico).WithOne(o => o.Cliente).HasForeignKey(o => o.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.DataCadastro).HasColumnType("timestamp without time zone");
        });

        // Veiculo
        modelBuilder.Entity<Veiculo>(entity =>
        {
            entity.ToTable("Veiculo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1000);
            entity.Property(e => e.Placa).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Marca).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Modelo).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Placa).IsUnique();
            entity.HasMany(e => e.OrdensDeServico).WithOne(o => o.Veiculo).HasForeignKey(o => o.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        });

        // Servico
        modelBuilder.Entity<Servico>(entity =>
        {
            entity.ToTable("Servico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1000);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Descricao).IsRequired();
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.Property(e => e.TempoEstimado).HasColumnName("TempoEstimadoMinutos");
            entity.HasMany(e => e.OrdensDeServico).WithOne(os => os.Servico).HasForeignKey(os => os.ServicoId).OnDelete(DeleteBehavior.Restrict);
        });

        // Peca
        modelBuilder.Entity<Peca>(entity =>
        {
            entity.ToTable("Peca");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1000);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.HasMany(e => e.OrdensDeServico).WithOne(op => op.Peca).HasForeignKey(op => op.PecaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.PedidosCompra).WithOne(pc => pc.Peca).HasForeignKey(pc => pc.PecaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.MovimentacoesEstoque).WithOne(m => m.Peca).HasForeignKey(m => m.PecaId).OnDelete(DeleteBehavior.Restrict);
        });

        // OrdemDeServico
        modelBuilder.Entity<OrdemDeServico>(entity =>
        {
            entity.ToTable("OrdemServico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 3000);
            entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DescricaoSolicitacao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ObservacoesRecepcao).HasMaxLength(1000);
            entity.Property(e => e.MotivoCancelamento).HasMaxLength(1000);
            entity.Property(e => e.OrcamentoEnviadoEm).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DataFinalizacao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DataPagamento).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.DataAbertura).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DataConclusao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasMany(e => e.Servicos).WithOne(os => os.OrdemDeServico).HasForeignKey(os => os.OrdemDeServicoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Pecas).WithOne(op => op.OrdemDeServico).HasForeignKey(op => op.OrdemDeServicoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Historicos).WithOne(h => h.OrdemDeServico).HasForeignKey(h => h.OrdemDeServicoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany<PedidoCompra>().WithOne(pc => pc.OrdemDeServico).HasForeignKey(pc => pc.OrdemDeServicoId).OnDelete(DeleteBehavior.Cascade);
        });

        // OrdemDeServicoServico
        modelBuilder.Entity<OrdemDeServicoServico>(entity =>
        {
            entity.ToTable("OrdemServicoItemServico");
            entity.HasKey(e => new { e.OrdemDeServicoId, e.ServicoId });
            entity.Property(e => e.OrdemDeServicoId).HasColumnName("OrdemServicoId");
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.Property(e => e.TempoEstimado).HasColumnName("TempoEstimadoMinutos");
        });

        // OrdemDeServicoPeca
        modelBuilder.Entity<OrdemDeServicoPeca>(entity =>
        {
            entity.ToTable("OrdemServicoItemPeca");
            entity.HasKey(e => new { e.OrdemDeServicoId, e.PecaId });
            entity.Property(e => e.OrdemDeServicoId).HasColumnName("OrdemServicoId");
            entity.Property(e => e.Preco).HasPrecision(18, 2);
        });

        // OrdemServicoHistorico
        modelBuilder.Entity<OrdemServicoHistorico>(entity =>
        {
            entity.ToTable("OrdemServicoHistorico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1);
            entity.Property(e => e.UsuarioId).HasMaxLength(100);
            entity.Property(e => e.UsuarioNome).HasMaxLength(255);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DataEvento).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StatusAnterior).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.StatusNovo).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.TipoEvento).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(e => e.OrdemDeServicoId);
            entity.HasIndex(e => e.DataEvento);
        });

        modelBuilder.Entity<PedidoCompra>(entity =>
        {
            entity.ToTable("PedidoCompra");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1);
            entity.Property(e => e.Observacao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DataSolicitacao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.DataRecebimento).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => new { e.OrdemDeServicoId, e.PecaId, e.Status });
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("MovimentacaoEstoque");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DataMovimentacao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TipoMovimentacao).IsRequired();
            entity.HasOne(e => e.OrdemDeServico)
                .WithMany()
                .HasForeignKey(e => e.OrdemDeServicoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PedidoCompra)
                .WithMany(pc => pc.MovimentacoesEstoque)
                .HasForeignKey(e => e.PedidoCompraId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.PecaId);
            entity.HasIndex(e => e.OrdemDeServicoId);
            entity.HasIndex(e => e.PedidoCompraId);
            entity.HasIndex(e => e.DataMovimentacao);
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(startValue: 1000);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.UsuarioLogin).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.UsuarioLogin).IsUnique();
        });
    }
}
