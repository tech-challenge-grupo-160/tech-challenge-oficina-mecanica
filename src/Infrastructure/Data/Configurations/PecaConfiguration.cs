using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
    public void Configure(EntityTypeBuilder<Peca> entity)
    {
        entity.ToTable("Peca");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 1000);
        entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
        entity.Property(e => e.Marca).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Modelo).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Preco).HasPrecision(18, 2);

        entity.HasMany(e => e.OrdensDeServico)
            .WithOne(op => op.Peca)
            .HasForeignKey(op => op.PecaId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(e => e.PedidosCompra)
            .WithOne(pc => pc.Peca)
            .HasForeignKey(pc => pc.PecaId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(e => e.MovimentacoesEstoque)
            .WithOne(m => m.Peca)
            .HasForeignKey(m => m.PecaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
