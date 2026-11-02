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
        builder.HasIndex(item => item.ProcessadoEm);
    }
}
