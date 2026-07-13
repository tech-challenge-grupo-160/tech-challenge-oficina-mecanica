using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class OrdemDeServicoPecaConfiguration : IEntityTypeConfiguration<OrdemDeServicoPeca>
{
    public void Configure(EntityTypeBuilder<OrdemDeServicoPeca> entity)
    {
        entity.ToTable("OrdemServicoItemPeca");
        entity.HasKey(e => new { e.OrdemDeServicoId, e.PecaId });
        entity.Property(e => e.OrdemDeServicoId).HasColumnName("OrdemServicoId");
        entity.Property(e => e.Preco).HasPrecision(18, 2);
    }
}
