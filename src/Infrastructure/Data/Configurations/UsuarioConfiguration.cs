using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> entity)
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
    }
}
