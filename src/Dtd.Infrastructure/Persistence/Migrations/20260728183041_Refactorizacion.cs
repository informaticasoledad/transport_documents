using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Refactorizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_consignees_documento_documentos_documento_id",
                table: "consignees_documento");

            migrationBuilder.DropTable(
                name: "documento_eventos");

            migrationBuilder.DropColumn(
                name: "consignee_destino_nombre",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "consignee_destino_telefono",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "destino_address_phone1",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "destino_almacen_destino",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "destino_municipio",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "es_directo",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "shipment_reference",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "actualizado_en",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "docuten_estado",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "envio_directo",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "usuario",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "calle",
                table: "almacenes");

            migrationBuilder.DropColumn(
                name: "municipio",
                table: "almacenes");

            migrationBuilder.DropColumn(
                name: "pais",
                table: "almacenes");

            migrationBuilder.RenameColumn(
                name: "destino_provincia",
                table: "envios",
                newName: "destino_telefono");

            migrationBuilder.RenameColumn(
                name: "destino_pais",
                table: "envios",
                newName: "destino_codigo");

            migrationBuilder.RenameColumn(
                name: "destino_address_street",
                table: "envios",
                newName: "destino_direccion");

            migrationBuilder.RenameColumn(
                name: "destino_address_name",
                table: "envios",
                newName: "destino_nombre");

            migrationBuilder.RenameColumn(
                name: "docuten_id",
                table: "documentos",
                newName: "usuario_generacion_id");

            migrationBuilder.AddColumn<Guid>(
                name: "consignee_id",
                table: "envios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_ciudad",
                table: "envios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_codigo_pais",
                table: "envios",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referencia",
                table: "envios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tipo_agrupacion",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_postal",
                table: "almacenes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ciudad",
                table: "almacenes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo_pais_iso",
                table: "almacenes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "direccion",
                table: "almacenes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_envios_consignee_id",
                table: "envios",
                column: "consignee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_envios_consignees_documento_consignee_id",
                table: "envios",
                column: "consignee_id",
                principalTable: "consignees_documento",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_envios_consignees_documento_consignee_id",
                table: "envios");

            migrationBuilder.DropIndex(
                name: "ix_envios_consignee_id",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "consignee_id",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "destino_ciudad",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "destino_codigo_pais",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "referencia",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "tipo_agrupacion",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "ciudad",
                table: "almacenes");

            migrationBuilder.DropColumn(
                name: "codigo_pais_iso",
                table: "almacenes");

            migrationBuilder.DropColumn(
                name: "direccion",
                table: "almacenes");

            migrationBuilder.RenameColumn(
                name: "destino_telefono",
                table: "envios",
                newName: "destino_provincia");

            migrationBuilder.RenameColumn(
                name: "destino_nombre",
                table: "envios",
                newName: "destino_address_name");

            migrationBuilder.RenameColumn(
                name: "destino_direccion",
                table: "envios",
                newName: "destino_address_street");

            migrationBuilder.RenameColumn(
                name: "destino_codigo",
                table: "envios",
                newName: "destino_pais");

            migrationBuilder.RenameColumn(
                name: "usuario_generacion_id",
                table: "documentos",
                newName: "docuten_id");

            migrationBuilder.AddColumn<string>(
                name: "consignee_destino_nombre",
                table: "envios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "consignee_destino_telefono",
                table: "envios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_address_phone1",
                table: "envios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_almacen_destino",
                table: "envios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "destino_municipio",
                table: "envios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "es_directo",
                table: "envios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "shipment_reference",
                table: "envios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "actualizado_en",
                table: "documentos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "docuten_estado",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "envio_directo",
                table: "documentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "usuario",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "codigo_postal",
                table: "almacenes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "calle",
                table: "almacenes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "municipio",
                table: "almacenes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pais",
                table: "almacenes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "documento_eventos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_http = table.Column<int>(type: "integer", nullable: true),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    momento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documento_eventos", x => x.id);
                    table.ForeignKey(
                        name: "fk_documento_eventos_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documento_eventos_documento_id",
                table: "documento_eventos",
                column: "documento_id");

            migrationBuilder.AddForeignKey(
                name: "fk_consignees_documento_documentos_documento_id",
                table: "consignees_documento",
                column: "documento_id",
                principalTable: "documentos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
