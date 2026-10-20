using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPorteadorDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "porteador_codigo_pais_iso",
                table: "documentos",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "porteador_codigo_postal",
                table: "documentos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "porteador_direccion",
                table: "documentos",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "porteador_identificador_fiscal",
                table: "documentos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "porteador_municipio",
                table: "documentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "porteador_nombre",
                table: "documentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "porteador_codigo_pais_iso",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "porteador_codigo_postal",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "porteador_direccion",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "porteador_identificador_fiscal",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "porteador_municipio",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "porteador_nombre",
                table: "documentos");
        }
    }
}
