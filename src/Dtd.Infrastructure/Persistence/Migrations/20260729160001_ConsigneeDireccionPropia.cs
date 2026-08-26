using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsigneeDireccionPropia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_pais_iso",
                table: "consignees",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_postal",
                table: "consignees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "direccion",
                table: "consignees",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "municipio",
                table: "consignees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_pais_iso",
                table: "consignees");

            migrationBuilder.DropColumn(
                name: "codigo_postal",
                table: "consignees");

            migrationBuilder.DropColumn(
                name: "direccion",
                table: "consignees");

            migrationBuilder.DropColumn(
                name: "municipio",
                table: "consignees");
        }
    }
}
