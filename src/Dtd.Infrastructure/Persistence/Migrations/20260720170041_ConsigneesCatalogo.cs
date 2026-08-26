using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsigneesCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consignees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consignees_documento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consignee_catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consignee_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignees_documento", x => x.id);
                    table.ForeignKey(
                        name: "fk_consignees_documento_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "almacen_agencia_consignees_defecto",
                columns: table => new
                {
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consignee_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacen_agencia_consignees_defecto", x => new { x.almacen_id, x.agencia_id, x.consignee_id });
                    table.ForeignKey(
                        name: "fk_almacen_agencia_consignees_defecto_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_consignees_defecto_almacenes_almacen_id",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_consignees_defecto_consignees_consignee_id",
                        column: x => x.consignee_id,
                        principalTable: "consignees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consignee_agencias",
                columns: table => new
                {
                    consignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignee_agencias", x => new { x.consignee_id, x.agencia_id });
                    table.ForeignKey(
                        name: "fk_consignee_agencias_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_consignee_agencias_consignees_consignee_id",
                        column: x => x.consignee_id,
                        principalTable: "consignees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consignee_almacenes",
                columns: table => new
                {
                    consignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignee_almacenes", x => new { x.consignee_id, x.almacen_id });
                    table.ForeignKey(
                        name: "fk_consignee_almacenes_almacenes_almacen_id",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_consignee_almacenes_consignees_consignee_id",
                        column: x => x.consignee_id,
                        principalTable: "consignees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_consignees_defecto_agencia_id",
                table: "almacen_agencia_consignees_defecto",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_consignees_defecto_consignee_id",
                table: "almacen_agencia_consignees_defecto",
                column: "consignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignee_agencias_agencia_id",
                table: "consignee_agencias",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignee_almacenes_almacen_id",
                table: "consignee_almacenes",
                column: "almacen_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignees_empresa_codigo",
                table: "consignees",
                columns: new[] { "empresa", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_consignees_documento_documento_id_consignee_catalog_id",
                table: "consignees_documento",
                columns: new[] { "documento_id", "consignee_catalog_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "almacen_agencia_consignees_defecto");

            migrationBuilder.DropTable(
                name: "consignee_agencias");

            migrationBuilder.DropTable(
                name: "consignee_almacenes");

            migrationBuilder.DropTable(
                name: "consignees_documento");

            migrationBuilder.DropTable(
                name: "consignees");
        }
    }
}
