using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class OrdemDeServicoConfiguration : IEntityTypeConfiguration<OrdemDeServico>
{
    public void Configure(EntityTypeBuilder<OrdemDeServico> entity)
    {
        entity.ToTable("OrdemServico");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 3000);
        entity.Property(e => e.Numero).IsRequired().HasMaxLength(50);
        entity.Property(e => e.CodigoAcompanhamento).IsRequired().HasMaxLength(40);
        entity.Property(e => e.TokenAcompanhamentoHash).IsRequired().HasMaxLength(64);
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
        entity.HasIndex(e => e.CodigoAcompanhamento).IsUnique();
        entity.HasIndex(e => e.Status);

        entity.HasMany(e => e.Servicos)
            .WithOne(os => os.OrdemDeServico)
            .HasForeignKey(os => os.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Pecas)
            .WithOne(op => op.OrdemDeServico)
            .HasForeignKey(op => op.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Historicos)
            .WithOne(h => h.OrdemDeServico)
            .HasForeignKey(h => h.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.NotificacoesCliente)
            .WithOne(n => n.OrdemDeServico)
            .HasForeignKey(n => n.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany<PedidoCompra>()
            .WithOne(pc => pc.OrdemDeServico)
            .HasForeignKey(pc => pc.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
