using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Api.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class AddAcompanhamentoPublicoOs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoAcompanhamento",
                table: "OrdemServico",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenAcompanhamentoHash",
                table: "OrdemServico",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET
                    "CodigoAcompanhamento" = CONCAT('AC-LEGACY-', LPAD("Id"::text, 8, '0')),
                    "TokenAcompanhamentoHash" = md5(random()::text || clock_timestamp()::text || "Id"::text) || md5(random()::text || clock_timestamp()::text || "Id"::text)
                WHERE "CodigoAcompanhamento" IS NULL OR "TokenAcompanhamentoHash" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoAcompanhamento",
                table: "OrdemServico",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenAcompanhamentoHash",
                table: "OrdemServico",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_CodigoAcompanhamento",
                table: "OrdemServico",
                column: "CodigoAcompanhamento",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrdemServico_CodigoAcompanhamento",
                table: "OrdemServico");

            migrationBuilder.DropColumn(
                name: "CodigoAcompanhamento",
                table: "OrdemServico");

            migrationBuilder.DropColumn(
                name: "TokenAcompanhamentoHash",
                table: "OrdemServico");
        }
    }
}
