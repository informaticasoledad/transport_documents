using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConductoresYAgenciasPorEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transportistas_defecto");

            migrationBuilder.DropIndex(
                name: "ix_agencias_codigo",
                table: "agencias");

            migrationBuilder.DropColumn(
                name: "transportista_email",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "transportista_erp_id",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "transportista_movil",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "transportista_nombre",
                table: "documentos");

            migrationBuilder.AddColumn<Guid>(
                name: "conductor_defecto_id",
                table: "almacen_agencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "empresa",
                table: "agencias",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "conductores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conductores", x => x.id);
                    table.ForeignKey(
                        name: "fk_conductores_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conductores_documento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conductor_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conductores_documento", x => x.id);
                    table.ForeignKey(
                        name: "fk_conductores_documento_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencias_conductor_defecto_id",
                table: "almacen_agencias",
                column: "conductor_defecto_id");

            migrationBuilder.CreateIndex(
                name: "ix_agencias_empresa_codigo",
                table: "agencias",
                columns: new[] { "empresa", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conductores_agencia_id_codigo",
                table: "conductores",
                columns: new[] { "agencia_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conductores_documento_documento_id_conductor_codigo",
                table: "conductores_documento",
                columns: new[] { "documento_id", "conductor_codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencias_conductores_conductor_defecto_id",
                table: "almacen_agencias",
                column: "conductor_defecto_id",
                principalTable: "conductores",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencias_conductores_conductor_defecto_id",
                table: "almacen_agencias");

            migrationBuilder.DropTable(
                name: "conductores");

            migrationBuilder.DropTable(
                name: "conductores_documento");

            migrationBuilder.DropIndex(
                name: "ix_almacen_agencias_conductor_defecto_id",
                table: "almacen_agencias");

            migrationBuilder.DropIndex(
                name: "ix_agencias_empresa_codigo",
                table: "agencias");

            migrationBuilder.DropColumn(
                name: "conductor_defecto_id",
                table: "almacen_agencias");

            migrationBuilder.DropColumn(
                name: "empresa",
                table: "agencias");

            migrationBuilder.AddColumn<string>(
                name: "transportista_email",
                table: "documentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transportista_erp_id",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transportista_movil",
                table: "documentos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transportista_nombre",
                table: "documentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "transportistas_defecto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    erp_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transportistas_defecto", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agencias_codigo",
                table: "agencias",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportistas_defecto_agencia_codigo_erp_id",
                table: "transportistas_defecto",
                columns: new[] { "agencia_codigo", "erp_id" },
                unique: true);
        }
    }
}

