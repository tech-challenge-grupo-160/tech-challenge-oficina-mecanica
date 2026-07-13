using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> entity)
    {
        entity.ToTable("Veiculo");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 1000);
        entity.Property(e => e.Placa)
            .HasConversion(
                vo => vo.Valor,
                str => PlacaVeiculo.FromDatabase(str))
            .IsRequired()
            .HasMaxLength(10);
        entity.Property(e => e.Marca).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Modelo).IsRequired().HasMaxLength(100);

        entity.HasIndex(e => e.Placa).IsUnique();

        entity.HasMany(e => e.OrdensDeServico)
            .WithOne(o => o.Veiculo)
            .HasForeignKey(o => o.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
