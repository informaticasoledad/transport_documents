using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecintoYMatriculaDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "matricula",
                table: "documentos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "precinto",
                table: "documentos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requiere_precinto",
                table: "documentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "peso_total",
                table: "documento_expediciones",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "requiere_precinto",
                table: "agencias",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "matricula",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "precinto",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "requiere_precinto",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "peso_total",
                table: "documento_expediciones");

            migrationBuilder.DropColumn(
                name: "requiere_precinto",
                table: "agencias");
        }
    }
}
