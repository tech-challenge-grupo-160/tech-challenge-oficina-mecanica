using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [DbContext(typeof(OficinaDbContext))]
    [Migration("20260418170000_AddDadosRecepcaoOrdemServico")]
    [ExcludeFromCodeCoverage]
    public partial class AddDadosRecepcaoOrdemServico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescricaoSolicitacao",
                table: "OrdemServico",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ObservacoesRecepcao",
                table: "OrdemServico",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "DescricaoSolicitacao" = 'Solicitacao nao informada.'
                WHERE "DescricaoSolicitacao" = '';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "OrdemServico"
                ALTER COLUMN "Id" RESTART WITH 3000;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescricaoSolicitacao",
                table: "OrdemServico");

            migrationBuilder.DropColumn(
                name: "ObservacoesRecepcao",
                table: "OrdemServico");

            migrationBuilder.Sql("""
                ALTER TABLE "OrdemServico"
                ALTER COLUMN "Id" RESTART WITH 1000;
                """);
        }
    }
}
