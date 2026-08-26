using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlmacenADocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_expediciones_empresa_erp_id",
                table: "expediciones");

            migrationBuilder.DropIndex(
                name: "ix_documentos_empresa_agencia_codigo",
                table: "documentos");

            migrationBuilder.AddColumn<string>(
                name: "almacen_codigo",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "almacen_codigo",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_expediciones_empresa_almacen_codigo_agencia_codigo_erp_id",
                table: "expediciones",
                columns: new[] { "empresa", "almacen_codigo", "agencia_codigo", "erp_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documentos_empresa_almacen_codigo_agencia_codigo",
                table: "documentos",
                columns: new[] { "empresa", "almacen_codigo", "agencia_codigo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_expediciones_empresa_almacen_codigo_agencia_codigo_erp_id",
                table: "expediciones");

            migrationBuilder.DropIndex(
                name: "ix_documentos_empresa_almacen_codigo_agencia_codigo",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "almacen_codigo",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "almacen_codigo",
                table: "documentos");

            migrationBuilder.CreateIndex(
                name: "ix_expediciones_empresa_erp_id",
                table: "expediciones",
                columns: new[] { "empresa", "erp_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documentos_empresa_agencia_codigo",
                table: "documentos",
                columns: new[] { "empresa", "agencia_codigo" });
        }
    }
}

