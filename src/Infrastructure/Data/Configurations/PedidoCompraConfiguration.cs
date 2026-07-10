using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class PedidoCompraConfiguration : IEntityTypeConfiguration<PedidoCompra>
{
    public void Configure(EntityTypeBuilder<PedidoCompra> entity)
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
    }
}
