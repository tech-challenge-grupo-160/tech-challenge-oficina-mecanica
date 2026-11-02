using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessage");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Tipo).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Destino).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.UltimoErro).HasMaxLength(2000);
        builder.HasIndex(item => new { item.PublicadoEm, item.CriadoEm });
    }
}
