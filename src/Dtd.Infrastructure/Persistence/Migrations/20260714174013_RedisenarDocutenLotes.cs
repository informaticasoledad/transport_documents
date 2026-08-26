using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedisenarDocutenLotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "transportistas_defecto",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_address_name",
                table: "expediciones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_address_street",
                table: "expediciones",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                table: "empresas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_id",
                table: "empresas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transportista_email",
                table: "documentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "transportistas_defecto");

            migrationBuilder.DropColumn(
                name: "destino_address_name",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "destino_address_street",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "nombre",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "tax_id",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "transportista_email",
                table: "documentos");
        }
    }
}

