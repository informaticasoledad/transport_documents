using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CcsCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ccs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ccs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ccs_documento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cc_catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cc_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ccs_documento", x => x.id);
                    table.ForeignKey(
                        name: "fk_ccs_documento_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "almacen_agencia_ccs_defecto",
                columns: table => new
                {
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cc_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacen_agencia_ccs_defecto", x => new { x.almacen_id, x.agencia_id, x.cc_id });
                    table.ForeignKey(
                        name: "fk_almacen_agencia_ccs_defecto_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_ccs_defecto_almacenes_almacen_id",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_ccs_defecto_ccs_cc_id",
                        column: x => x.cc_id,
                        principalTable: "ccs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cc_agencias",
                columns: table => new
                {
                    cc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cc_agencias", x => new { x.cc_id, x.agencia_id });
                    table.ForeignKey(
                        name: "fk_cc_agencias_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cc_agencias_ccs_cc_id",
                        column: x => x.cc_id,
                        principalTable: "ccs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cc_almacenes",
                columns: table => new
                {
                    cc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cc_almacenes", x => new { x.cc_id, x.almacen_id });
                    table.ForeignKey(
                        name: "fk_cc_almacenes_almacenes_almacen_id",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cc_almacenes_ccs_cc_id",
                        column: x => x.cc_id,
                        principalTable: "ccs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_ccs_defecto_agencia_id",
                table: "almacen_agencia_ccs_defecto",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_ccs_defecto_cc_id",
                table: "almacen_agencia_ccs_defecto",
                column: "cc_id");

            migrationBuilder.CreateIndex(
                name: "ix_cc_agencias_agencia_id",
                table: "cc_agencias",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_cc_almacenes_almacen_id",
                table: "cc_almacenes",
                column: "almacen_id");

            migrationBuilder.CreateIndex(
                name: "ix_ccs_empresa_codigo",
                table: "ccs",
                columns: new[] { "empresa", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ccs_documento_documento_id_cc_catalog_id",
                table: "ccs_documento",
                columns: new[] { "documento_id", "cc_catalog_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "almacen_agencia_ccs_defecto");

            migrationBuilder.DropTable(
                name: "cc_agencias");

            migrationBuilder.DropTable(
                name: "cc_almacenes");

            migrationBuilder.DropTable(
                name: "ccs_documento");

            migrationBuilder.DropTable(
                name: "ccs");
        }
    }
}
