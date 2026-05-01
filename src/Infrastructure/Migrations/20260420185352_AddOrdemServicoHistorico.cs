using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddOrdemServicoHistorico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdemServicoHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemDeServicoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioNome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StatusAnterior = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StatusNovo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoEvento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataEvento = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServicoHistorico_OrdemServico_OrdemDeServicoId",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoHistorico_DataEvento",
                table: "OrdemServicoHistorico",
                column: "DataEvento");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoHistorico_OrdemDeServicoId",
                table: "OrdemServicoHistorico",
                column: "OrdemDeServicoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdemServicoHistorico");
        }
    }
}
