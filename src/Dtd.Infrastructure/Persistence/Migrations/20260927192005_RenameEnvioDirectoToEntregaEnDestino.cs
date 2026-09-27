using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameEnvioDirectoToEntregaEnDestino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "expedition_type",
                table: "documento_expediciones",
                newName: "tipo_expedicion");

            migrationBuilder.RenameColumn(
                name: "envio_directo",
                table: "agencias",
                newName: "entrega_en_destino");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tipo_expedicion",
                table: "documento_expediciones",
                newName: "expedition_type");

            migrationBuilder.RenameColumn(
                name: "entrega_en_destino",
                table: "agencias",
                newName: "envio_directo");
        }
    }
}
