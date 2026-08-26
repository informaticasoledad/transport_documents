using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnviosYEnvioDirecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "destino_address_phone1",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "envio_id",
                table: "expediciones",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "envio_directo",
                table: "documentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "dir_calle",
                table: "consignees_documento",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_codigo_postal",
                table: "consignees_documento",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_municipio",
                table: "consignees_documento",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_pais",
                table: "consignees_documento",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_calle",
                table: "consignee_almacenes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_codigo_postal",
                table: "consignee_almacenes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_municipio",
                table: "consignee_almacenes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_pais",
                table: "consignee_almacenes",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_calle",
                table: "consignee_agencias",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_codigo_postal",
                table: "consignee_agencias",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_municipio",
                table: "consignee_agencias",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir_pais",
                table: "consignee_agencias",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "envio_directo",
                table: "agencias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "envios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    shipment_reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    bultos = table.Column<int>(type: "integer", nullable: false),
                    destino_pais = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_provincia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    destino_municipio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_almacen_destino = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    destino_address_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    destino_address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    destino_address_phone1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consignee_destino_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    consignee_destino_telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    es_directo = table.Column<bool>(type: "boolean", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_envios", x => x.id);
                    table.ForeignKey(
                        name: "fk_envios_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_expediciones_envio_id",
                table: "expediciones",
                column: "envio_id");

            migrationBuilder.CreateIndex(
                name: "ix_envios_documento_id",
                table: "envios",
                column: "documento_id");

            migrationBuilder.AddForeignKey(
                name: "fk_expediciones_envios_envio_id",
                table: "expediciones",
                column: "envio_id",
                principalTable: "envios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_expediciones_envios_envio_id",
                table: "expediciones");

            migrationBuilder.DropTable(
                name: "envios");

            migrationBuilder.DropIndex(
                name: "ix_expediciones_envio_id",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "destino_address_phone1",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "envio_id",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "envio_directo",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "dir_calle",
                table: "consignees_documento");

            migrationBuilder.DropColumn(
                name: "dir_codigo_postal",
                table: "consignees_documento");

            migrationBuilder.DropColumn(
                name: "dir_municipio",
                table: "consignees_documento");

            migrationBuilder.DropColumn(
                name: "dir_pais",
                table: "consignees_documento");

            migrationBuilder.DropColumn(
                name: "dir_calle",
                table: "consignee_almacenes");

            migrationBuilder.DropColumn(
                name: "dir_codigo_postal",
                table: "consignee_almacenes");

            migrationBuilder.DropColumn(
                name: "dir_municipio",
                table: "consignee_almacenes");

            migrationBuilder.DropColumn(
                name: "dir_pais",
                table: "consignee_almacenes");

            migrationBuilder.DropColumn(
                name: "dir_calle",
                table: "consignee_agencias");

            migrationBuilder.DropColumn(
                name: "dir_codigo_postal",
                table: "consignee_agencias");

            migrationBuilder.DropColumn(
                name: "dir_municipio",
                table: "consignee_agencias");

            migrationBuilder.DropColumn(
                name: "dir_pais",
                table: "consignee_agencias");

            migrationBuilder.DropColumn(
                name: "envio_directo",
                table: "agencias");
        }
    }
}
