using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Fiap.TechChallenge.OficinaMecanica.Api.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class ReplaceMd5PasswordHashWithBCrypt : Migration
    {
        private const string AdminBCryptHash = "$2a$12$EzjaTN23pfQI/XJo.jRuge9qe8SS7eKnypVwNiRK9HftK1H2K1JAy";
        private const string AdminMd5Hash = "0192023A7BBD73250516F069DF18B500";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Usuario",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.Sql($"""
                UPDATE "Usuario"
                SET "SenhaHash" = '{AdminBCryptHash}'
                WHERE "UsuarioLogin" = 'admin'
                  AND "SenhaHash" = '{AdminMd5Hash}';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                UPDATE "Usuario"
                SET "SenhaHash" = '{AdminMd5Hash}'
                WHERE "UsuarioLogin" = 'admin'
                  AND "SenhaHash" = '{AdminBCryptHash}';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Usuario",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
