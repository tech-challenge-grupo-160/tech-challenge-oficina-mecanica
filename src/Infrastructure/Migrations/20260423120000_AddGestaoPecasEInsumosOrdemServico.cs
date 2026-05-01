using System;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    [DbContext(typeof(OficinaDbContext))]
    [Migration("20260423120000_AddGestaoPecasEInsumosOrdemServico")]
    public partial class AddGestaoPecasEInsumosOrdemServico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidoCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdemDeServicoId = table.Column<int>(type: "integer", nullable: false),
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeSolicitada = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeRecebida = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
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
                    PecaId = table.Column<int>(type: "integer", nullable: false),
                    OrdemDeServicoId = table.Column<int>(type: "integer", nullable: true),
                    PedidoCompraId = table.Column<int>(type: "integer", nullable: true),
                    TipoMovimentacao = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeAnterior = table.Column<int>(type: "integer", nullable: false),
                    QuantidadePosterior = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataMovimentacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "IX_PedidoCompra_OrdemDeServicoId_PecaId_Status",
                table: "PedidoCompra",
                columns: new[] { "OrdemDeServicoId", "PecaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoCompra_PecaId",
                table: "PedidoCompra",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacaoEstoque");

            migrationBuilder.DropTable(
                name: "PedidoCompra");
        }
    }
}
