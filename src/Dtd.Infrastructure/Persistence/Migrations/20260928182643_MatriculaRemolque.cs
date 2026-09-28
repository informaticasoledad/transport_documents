using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatriculaRemolque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "matricula_remolque",
                table: "documentos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trailer_license_plate",
                table: "documento_conductores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trailer_license_plate",
                table: "conductores",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "matricula_remolque",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "trailer_license_plate",
                table: "documento_conductores");

            migrationBuilder.DropColumn(
                name: "trailer_license_plate",
                table: "conductores");
        }
    }
}
