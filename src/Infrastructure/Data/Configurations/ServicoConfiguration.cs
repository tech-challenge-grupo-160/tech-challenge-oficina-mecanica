using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> entity)
    {
        entity.ToTable("Servico");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 1000);
        entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
        entity.Property(e => e.Descricao).IsRequired();
        entity.Property(e => e.Preco).HasPrecision(18, 2);
        entity.Property(e => e.TempoEstimado).HasColumnName("TempoEstimadoMinutos");

        entity.HasMany(e => e.OrdensDeServico)
            .WithOne(os => os.Servico)
            .HasForeignKey(os => os.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
