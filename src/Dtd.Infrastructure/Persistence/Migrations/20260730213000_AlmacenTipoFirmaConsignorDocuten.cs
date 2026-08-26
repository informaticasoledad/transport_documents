using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlmacenTipoFirmaConsignorDocuten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_firma_consignor",
                table: "almacenes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "biometric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo_firma_consignor",
                table: "almacenes");
        }
    }
}
