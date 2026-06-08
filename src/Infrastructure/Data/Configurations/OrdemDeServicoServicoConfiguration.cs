using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class OrdemDeServicoServicoConfiguration : IEntityTypeConfiguration<OrdemDeServicoServico>
{
    public void Configure(EntityTypeBuilder<OrdemDeServicoServico> entity)
    {
        entity.ToTable("OrdemServicoItemServico");
        entity.HasKey(e => new { e.OrdemDeServicoId, e.ServicoId });
        entity.Property(e => e.OrdemDeServicoId).HasColumnName("OrdemServicoId");
        entity.Property(e => e.Preco).HasPrecision(18, 2);
        entity.Property(e => e.TempoEstimado).HasColumnName("TempoEstimadoMinutos");
    }
}
