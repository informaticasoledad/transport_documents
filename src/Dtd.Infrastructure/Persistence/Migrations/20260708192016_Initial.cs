using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agencias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    agencia_qs = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agencias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    agencia_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    almacen_origen = table.Column<int>(type: "integer", nullable: true),
                    fecha_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    docuten_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    docuten_estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fecha_generacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documentos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transportistas_defecto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    erp_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportistas_defecto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expediciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    erp_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    empresa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    agencia_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cliente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_pais = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_provincia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    destino_municipio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_almacen_destino = table.Column<int>(type: "integer", nullable: true),
                    peso = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    bultos = table.Column<int>(type: "integer", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(11,2)", precision: 11, scale: 2, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    manual = table.Column<bool>(type: "boolean", nullable: false),
                    transportista_erp_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transportista_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transportista_movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expediciones", x => x.id);
                    table.ForeignKey(
                        name: "fk_expediciones_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agencias_codigo",
                table: "agencias",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documentos_empresa_agencia_codigo",
                table: "documentos",
                columns: new[] { "empresa", "agencia_codigo" });

            migrationBuilder.CreateIndex(
                name: "ix_expediciones_documento_id",
                table: "expediciones",
                column: "documento_id");

            migrationBuilder.CreateIndex(
                name: "ix_expediciones_empresa_erp_id",
                table: "expediciones",
                columns: new[] { "empresa", "erp_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportistas_defecto_agencia_codigo_erp_id",
                table: "transportistas_defecto",
                columns: new[] { "agencia_codigo", "erp_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agencias");

            migrationBuilder.DropTable(
                name: "expediciones");

            migrationBuilder.DropTable(
                name: "transportistas_defecto");

            migrationBuilder.DropTable(
                name: "documentos");
        }
    }
}

