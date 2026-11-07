using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Api.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaOrcamentoDecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrcamentoId",
                table: "SagaInicioDiagnostico",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrcamentoId",
                table: "SagaInicioDiagnostico");
        }
    }
}
