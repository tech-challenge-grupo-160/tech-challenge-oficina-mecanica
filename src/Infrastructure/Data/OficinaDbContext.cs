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
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CpfCnpj).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.CpfCnpj).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasMany(e => e.Veiculos).WithOne(v => v.Cliente).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.OrdensDeServico).WithOne(o => o.Cliente).HasForeignKey(o => o.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        // Veiculo
        modelBuilder.Entity<Veiculo>(entity =>
        {
            entity.ToTable("Veiculo");
            entity.HasKey(e => e.Id);
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
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.HasMany(e => e.OrdensDeServico).WithOne(op => op.Peca).HasForeignKey(op => op.PecaId).OnDelete(DeleteBehavior.Restrict);
        });

        // OrdemDeServico
        modelBuilder.Entity<OrdemDeServico>(entity =>
        {
            entity.ToTable("OrdemServico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.DataAbertura)
                .HasConversion(
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(e => e.DataConclusao)
                .HasConversion(
                    v => v.HasValue && v.Value.Kind == DateTimeKind.Utc ? v : (v.HasValue ? v.Value.ToUniversalTime() : null),
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasMany(e => e.Servicos).WithOne(os => os.OrdemDeServico).HasForeignKey(os => os.OrdemDeServicoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Pecas).WithOne(op => op.OrdemDeServico).HasForeignKey(op => op.OrdemDeServicoId).OnDelete(DeleteBehavior.Cascade);
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

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
            entity.Property(e => e.UsuarioLogin).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.UsuarioLogin).IsUnique();
        });
    }
}
