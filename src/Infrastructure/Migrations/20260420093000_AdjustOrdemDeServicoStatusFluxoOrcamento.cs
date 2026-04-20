using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [DbContext(typeof(OficinaDbContext))]
    [Migration("20260420093000_AdjustOrdemDeServicoStatusFluxoOrcamento")]
    public partial class AdjustOrdemDeServicoStatusFluxoOrcamento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "Status" = "Status" + 1
                WHERE "Status" >= 2;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "Status" = "Status" - 1
                WHERE "Status" >= 3;
                """);
        }
    }
}
