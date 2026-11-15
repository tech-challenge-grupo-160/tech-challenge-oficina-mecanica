using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessage");
        builder.HasKey(item => item.MessageId);
        builder.Property(item => item.Tipo).HasMaxLength(200).IsRequired();
        builder.Property(item => item.TraceContext).HasMaxLength(2000);
        builder.HasIndex(item => item.ProcessadoEm);
        // O publicador da Outbox busca o trace da Saga pelo correlationId (F4-31).
        builder.HasIndex(item => item.CorrelationId);
    }
}
