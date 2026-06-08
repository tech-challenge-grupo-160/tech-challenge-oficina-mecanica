using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> entity)
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
    }
}
