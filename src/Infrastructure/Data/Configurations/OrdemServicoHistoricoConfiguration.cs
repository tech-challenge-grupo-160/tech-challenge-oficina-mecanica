using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class OrdemServicoHistoricoConfiguration : IEntityTypeConfiguration<OrdemServicoHistorico>
{
    public void Configure(EntityTypeBuilder<OrdemServicoHistorico> entity)
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
    }
}
