using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorExpedicionYOrigenDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "importe",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "manual",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "observaciones",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "peso",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "transportista_movil",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "transportista_nombre",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "almacen_origen",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "transportista_erp_id",
                table: "expediciones");

            migrationBuilder.AddColumn<string>(
                name: "expedition_code",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "erp_id",
                table: "expediciones",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "destino_almacen_destino",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_number",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expedition_type",
                table: "expediciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "origen_address_name",
                table: "documentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_address_phone1",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_address_street",
                table: "documentos",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_city",
                table: "documentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_country_iso_code",
                table: "documentos",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_country_name",
                table: "documentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_province_name",
                table: "documentos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_warehouse_id",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_zipcode",
                table: "documentos",
                type: "character varying(20)",
                maxLength: 20,
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_number",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "expedition_type",
                table: "expediciones");

            migrationBuilder.DropColumn(
                name: "origen_address_name",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_address_phone1",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_address_street",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_city",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_country_iso_code",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_country_name",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_province_name",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_warehouse_id",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "origen_zipcode",
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

            migrationBuilder.DropColumn(
                name: "expedition_code",
                table: "expediciones");

            migrationBuilder.AddColumn<string>(
                name: "transportista_erp_id",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "erp_id",
                table: "expediciones",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<int>(
                name: "destino_almacen_destino",
                table: "expediciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "importe",
                table: "expediciones",
                type: "numeric(11,2)",
                precision: 11,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "manual",
                table: "expediciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "observaciones",
                table: "expediciones",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "peso",
                table: "expediciones",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "transportista_movil",
                table: "expediciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transportista_nombre",
                table: "expediciones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "almacen_origen",
                table: "documentos",
                type: "integer",
                nullable: true);
        }
    }
}

