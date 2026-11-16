using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceContextMensageria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TraceContext",
                table: "OutboxMessage",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceContext",
                table: "InboxMessage",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessage_CorrelationId",
                table: "InboxMessage",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InboxMessage_CorrelationId",
                table: "InboxMessage");

            migrationBuilder.DropColumn(
                name: "TraceContext",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "TraceContext",
                table: "InboxMessage");
        }
    }
}
