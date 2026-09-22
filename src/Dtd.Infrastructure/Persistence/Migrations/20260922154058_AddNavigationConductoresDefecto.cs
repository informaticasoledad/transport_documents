using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationConductoresDefecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencia_conductores_defecto_almacen_agencias_almace",
                table: "almacen_agencia_conductores_defecto",
                columns: new[] { "almacen_id", "agencia_id" },
                principalTable: "almacen_agencias",
                principalColumns: new[] { "almacen_id", "agencia_id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencia_conductores_defecto_almacen_agencias_almace",
                table: "almacen_agencia_conductores_defecto");
        }
    }
}
