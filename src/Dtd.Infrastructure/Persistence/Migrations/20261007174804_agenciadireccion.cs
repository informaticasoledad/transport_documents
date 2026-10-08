using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class agenciadireccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_pais_iso",
                table: "agencias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_postal",
                table: "agencias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "direccion",
                table: "agencias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "municipio",
                table: "agencias",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_pais_iso",
                table: "agencias");

            migrationBuilder.DropColumn(
                name: "codigo_postal",
                table: "agencias");

            migrationBuilder.DropColumn(
                name: "direccion",
                table: "agencias");

            migrationBuilder.DropColumn(
                name: "municipio",
                table: "agencias");
        }
    }
}
