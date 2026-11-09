using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Api.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyCatalogStockAndPurchasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoItemPeca_Peca_PecaId",
                table: "OrdemServicoItemPeca");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicoItemServico_Servico_ServicoId",
                table: "OrdemServicoItemServico");

            migrationBuilder.DropTable(
                name: "MovimentacaoEstoque");

            migrationBuilder.DropTable(
                name: "Servico");

            migrationBuilder.DropTable(
                name: "PedidoCompra");

            migrationBuilder.DropTable(
                name: "Peca");

            migrationBuilder.DropIndex(
                name: "IX_OrdemServicoItemServico_ServicoId",
                table: "OrdemServicoItemServico");

            migrationBuilder.DropIndex(
                name: "IX_OrdemServicoItemPeca_PecaId",
                table: "OrdemServicoItemPeca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Peca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantidadeEstoque = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TempoEstimadoMinutos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PedidoCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemDeServicoId = table.Column<int>(type: "integer", nullable: false),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuantidadeRecebida = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeSolicitada = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoCompra_OrdemServico_OrdemDeServicoId",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoCompra_Peca_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Peca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacaoEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemDeServicoId = table.Column<int>(type: "integer", nullable: true),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    PedidoCompraId = table.Column<int>(type: "integer", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeAnterior = table.Column<int>(type: "integer", nullable: false),
                    QuantidadePosterior = table.Column<int>(type: "integer", nullable: false),
                    TipoMovimentacao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacaoEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacaoEstoque_OrdemServico_OrdemDeServicoId",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacaoEstoque_Peca_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Peca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacaoEstoque_PedidoCompra_PedidoCompraId",
                        column: x => x.PedidoCompraId,
                        principalTable: "PedidoCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoItemServico_ServicoId",
                table: "OrdemServicoItemServico",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoItemPeca_PecaId",
                table: "OrdemServicoItemPeca",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoEstoque_DataMovimentacao",
                table: "MovimentacaoEstoque",
                column: "DataMovimentacao");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoEstoque_OrdemDeServicoId",
                table: "MovimentacaoEstoque",
                column: "OrdemDeServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoEstoque_PecaId",
                table: "MovimentacaoEstoque",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacaoEstoque_PedidoCompraId",
                table: "MovimentacaoEstoque",
                column: "PedidoCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoCompra_OrdemDeServicoId_PecaId_Status",
                table: "PedidoCompra",
                columns: new[] { "OrdemDeServicoId", "PecaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoCompra_PecaId",
                table: "PedidoCompra",
                column: "PecaId");

            migrationBuilder.Sql("""
                INSERT INTO "Servico" ("Id", "Descricao", "Nome", "Preco", "TempoEstimadoMinutos")
                SELECT DISTINCT "ServicoId", 'Registro restaurado por rollback',
                       'Servico ' || "ServicoId", 0, 1
                FROM "OrdemServicoItemServico";

                INSERT INTO "Peca" ("Id", "Marca", "Modelo", "Nome", "Preco", "QuantidadeEstoque")
                SELECT DISTINCT "PecaId", 'Restaurada', 'Rollback',
                       'Peca ' || "PecaId", 0, 0
                FROM "OrdemServicoItemPeca";

                SELECT setval(pg_get_serial_sequence('"Servico"', 'Id'),
                    GREATEST(COALESCE((SELECT MAX("Id") FROM "Servico"), 999), 999), true);
                SELECT setval(pg_get_serial_sequence('"Peca"', 'Id'),
                    GREATEST(COALESCE((SELECT MAX("Id") FROM "Peca"), 999), 999), true);
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoItemPeca_Peca_PecaId",
                table: "OrdemServicoItemPeca",
                column: "PecaId",
                principalTable: "Peca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicoItemServico_Servico_ServicoId",
                table: "OrdemServicoItemServico",
                column: "ServicoId",
                principalTable: "Servico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
