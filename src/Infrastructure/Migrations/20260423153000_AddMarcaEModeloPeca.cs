using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [DbContext(typeof(OficinaDbContext))]
    [Migration("20260423153000_AddMarcaEModeloPeca")]
    [ExcludeFromCodeCoverage]
    public partial class AddMarcaEModeloPeca : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Peca",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Modelo",
                table: "Peca",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Peca"
                SET "Marca" = CASE "Nome"
                    WHEN 'Filtro de Oleo' THEN 'Mann'
                    WHEN 'Filtro de Ar' THEN 'Bosch'
                    WHEN 'Pastilha de Freio' THEN 'Cobreq'
                    WHEN 'Pneu Aro 15' THEN 'Pirelli'
                    WHEN 'Vela de Ignicao' THEN 'NGK'
                    ELSE 'Generica'
                END,
                "Modelo" = CASE "Nome"
                    WHEN 'Filtro de Oleo' THEN 'W610/3'
                    WHEN 'Filtro de Ar' THEN '0986AF'
                    WHEN 'Pastilha de Freio' THEN 'N-1234'
                    WHEN 'Pneu Aro 15' THEN '175/65R15'
                    WHEN 'Vela de Ignicao' THEN 'BKR6E'
                    ELSE 'Padrao'
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Peca");

            migrationBuilder.DropColumn(
                name: "Modelo",
                table: "Peca");
        }
    }
}
