using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> entity)
    {
        entity.ToTable("Cliente");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasIdentityOptions(startValue: 1000);
        entity.Property(e => e.Nome).IsRequired().HasMaxLength(255);
        entity.Property(e => e.CpfCnpj)
            .HasConversion(
                vo => vo.Valor,
                str => Documento.FromDatabase(str))
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(e => e.Telefone)
            .HasConversion(
                vo => vo.Valor,
                str => Telefone.FromDatabase(str))
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(e => e.Email)
            .HasConversion(
                vo => vo.Valor,
                str => Email.FromDatabase(str))
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue(StatusCliente.Ativo);

        entity.Property(e => e.DataCadastro).HasColumnType("timestamp without time zone");

        entity.HasIndex(e => e.CpfCnpj).IsUnique();
        entity.HasIndex(e => e.Email);
        entity.HasIndex(e => e.Status);

        entity.HasMany(e => e.Veiculos)
            .WithOne(v => v.Cliente)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(e => e.OrdensDeServico)
            .WithOne(o => o.Cliente)
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
