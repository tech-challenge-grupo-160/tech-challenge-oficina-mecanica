using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Api.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Cliente",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Status",
                table: "Cliente",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cliente_Status",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cliente");
        }
    }
}
