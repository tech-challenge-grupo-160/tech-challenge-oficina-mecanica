using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [DbContext(typeof(OficinaDbContext))]
    [Migration("20260420113000_RevertFluxoOrcamentoEmProcesso")]
    public partial class RevertFluxoOrcamentoEmProcesso : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "Status" = 1
                WHERE "Status" = 2;
                """);

            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "Status" = "Status" - 1
                WHERE "Status" >= 3;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "Status" = "Status" + 1
                WHERE "Status" >= 2;
                """);
            migrationBuilder.Sql("""
                UPDATE "OrdemServico"
                SET "Status" = 2
                WHERE "Status" = 1 AND "OrcamentoEnviadoEm" IS NULL;
                """);
        }
    }
}
