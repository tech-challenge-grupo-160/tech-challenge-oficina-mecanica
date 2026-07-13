using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class NotificacaoClienteConfiguration : IEntityTypeConfiguration<NotificacaoCliente>
{
    public void Configure(EntityTypeBuilder<NotificacaoCliente> entity)
    {
        entity.ToTable("NotificacaoCliente");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 1);
        entity.Property(e => e.DataNotificacao).HasColumnType("timestamp without time zone");
        entity.Property(e => e.Canal).HasConversion<string>().HasMaxLength(50);
        entity.Property(e => e.TipoNotificacao).HasConversion<string>().HasMaxLength(50);
        entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(2000);

        entity.HasIndex(e => e.OrdemDeServicoId);
        entity.HasIndex(e => e.DataNotificacao);
    }
}
