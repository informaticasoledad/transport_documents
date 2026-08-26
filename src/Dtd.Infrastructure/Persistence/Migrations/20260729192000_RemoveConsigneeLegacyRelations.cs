using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConsigneeLegacyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS consignee_agencias;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS consignee_almacenes;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consignee_agencias",
                columns: table => new
                {
                    consignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dir_pais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    dir_codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dir_calle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    dir_municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
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
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dir_pais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    dir_codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dir_calle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    dir_municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
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
                name: "ix_consignee_agencias_agencia_id",
                table: "consignee_agencias",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_consignee_almacenes_almacen_id",
                table: "consignee_almacenes",
                column: "almacen_id");
        }
    }
}
