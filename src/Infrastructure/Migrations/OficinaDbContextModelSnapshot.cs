using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [DbContext(typeof(OficinaDbContext))]
    partial class OficinaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Cliente", b =>
            {
                b.Property<Guid>("Id");
                b.Property<string>("CpfCnpj").IsRequired().HasMaxLength(20);
                b.Property<DateTime>("DataCadastro");
                b.Property<string>("Email").IsRequired().HasMaxLength(255);
                b.Property<string>("Nome").IsRequired().HasMaxLength(255);
                b.Property<string>("Telefone").IsRequired().HasMaxLength(20);
                b.HasKey("Id");
                b.HasIndex("CpfCnpj").IsUnique();
                b.HasIndex("Email");
                b.ToTable("Clientes");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Veiculo", b =>
            {
                b.Property<Guid>("Id");
                b.Property<int>("Ano");
                b.Property<Guid>("ClienteId");
                b.Property<string>("Marca").IsRequired().HasMaxLength(100);
                b.Property<string>("Modelo").IsRequired().HasMaxLength(100);
                b.Property<string>("Placa").IsRequired().HasMaxLength(10);
                b.HasKey("Id");
                b.HasIndex("Placa").IsUnique();
                b.HasIndex("ClienteId");
                b.ToTable("Veiculos");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Servico", b =>
            {
                b.Property<Guid>("Id");
                b.Property<int>("TempoEstimado");
                b.Property<decimal>("Preco");
                b.Property<string>("Descricao").IsRequired();
                b.Property<string>("Nome").IsRequired().HasMaxLength(255);
                b.HasKey("Id");
                b.ToTable("Servicos");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Peca", b =>
            {
                b.Property<Guid>("Id");
                b.Property<int>("QuantidadeEstoque");
                b.Property<decimal>("Preco");
                b.Property<string>("Nome").IsRequired().HasMaxLength(255);
                b.HasKey("Id");
                b.ToTable("Pecas");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServico", b =>
            {
                b.Property<Guid>("Id");
                b.Property<DateTime>("DataAbertura");
                b.Property<DateTime?>("DataConclusao");
                b.Property<Guid>("ClienteId");
                b.Property<Guid>("VeiculoId");
                b.Property<string>("Numero").IsRequired().HasMaxLength(50);
                b.Property<int>("Status");
                b.Property<decimal>("ValorTotal");
                b.HasKey("Id");
                b.HasIndex("Numero").IsUnique();
                b.HasIndex("Status");
                b.HasIndex("ClienteId");
                b.HasIndex("VeiculoId");
                b.ToTable("OrdensDeServico");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServicoServico", b =>
            {
                b.Property<Guid>("OrdemDeServicoId");
                b.Property<Guid>("ServicoId");
                b.Property<int>("TempoEstimado");
                b.Property<decimal>("Preco");
                b.HasKey("OrdemDeServicoId", "ServicoId");
                b.HasIndex("ServicoId");
                b.ToTable("OrdemDeServicoServicos");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServicoPeca", b =>
            {
                b.Property<Guid>("OrdemDeServicoId");
                b.Property<Guid>("PecaId");
                b.Property<int>("Quantidade");
                b.Property<decimal>("Preco");
                b.HasKey("OrdemDeServicoId", "PecaId");
                b.HasIndex("PecaId");
                b.ToTable("OrdemDeServicoPecas");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Veiculo", b =>
            {
                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Cliente")
                    .WithMany("Veiculos")
                    .HasForeignKey("ClienteId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServico", b =>
            {
                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Cliente")
                    .WithMany("OrdensDeServico")
                    .HasForeignKey("ClienteId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Veiculo")
                    .WithMany("OrdensDeServico")
                    .HasForeignKey("VeiculoId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServicoServico", b =>
            {
                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServico")
                    .WithMany("Servicos")
                    .HasForeignKey("OrdemDeServicoId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Servico")
                    .WithMany()
                    .HasForeignKey("ServicoId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServicoPeca", b =>
            {
                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServico")
                    .WithMany("Pecas")
                    .HasForeignKey("OrdemDeServicoId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Peca")
                    .WithMany()
                    .HasForeignKey("PecaId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Cliente", b =>
            {
                b.Navigation("OrdensDeServico");

                b.Navigation("Veiculos");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.OrdemDeServico", b =>
            {
                b.Navigation("Pecas");

                b.Navigation("Servicos");
            });

            modelBuilder.Entity("Fiap.TechChallenge.OficinaMecanica.Domain.Entities.Veiculo", b =>
            {
                b.Navigation("OrdensDeServico");
            });
        }
    }
}
