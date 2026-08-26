using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnviosDestinoEntregaSinConsigneesDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_envios_consignees_documento_consignee_id",
                table: "envios");

            migrationBuilder.DropTable(
                name: "consignees_documento");

            migrationBuilder.DropIndex(
                name: "ix_envios_consignee_id",
                table: "envios");

            migrationBuilder.DropColumn(
                name: "consignee_id",
                table: "envios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "consignee_id",
                table: "envios",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "consignees_documento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consignee_catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consignee_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    dir_pais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    dir_codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dir_calle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    dir_municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    movil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consignees_documento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_envios_consignee_id",
                table: "envios",
                column: "consignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignees_documento_documento_id_consignee_catalog_id",
                table: "consignees_documento",
                columns: new[] { "documento_id", "consignee_catalog_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_envios_consignees_documento_consignee_id",
                table: "envios",
                column: "consignee_id",
                principalTable: "consignees_documento",
                principalColumn: "id");
        }
    }
}
