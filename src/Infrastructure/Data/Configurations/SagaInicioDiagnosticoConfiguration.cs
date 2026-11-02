using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class SagaInicioDiagnosticoConfiguration
    : IEntityTypeConfiguration<SagaInicioDiagnostico>
{
    public void Configure(EntityTypeBuilder<SagaInicioDiagnostico> builder)
    {
        builder.ToTable("SagaInicioDiagnostico");
        builder.HasKey(item => item.CorrelationId);
        builder.Property(item => item.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(item => item.OrdemServicoId).IsUnique();
    }
}
