using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlmacenesYRelacionAgencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "almacenes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    calle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    municipio = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pais = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacenes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "almacen_agencias",
                columns: table => new
                {
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacen_agencias", x => new { x.almacen_id, x.agencia_id });
                    table.ForeignKey(
                        name: "fk_almacen_agencias_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencias_almacenes_almacen_id",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencias_agencia_id",
                table: "almacen_agencias",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_almacenes_empresa_codigo",
                table: "almacenes",
                columns: new[] { "empresa", "codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "almacen_agencias");

            migrationBuilder.DropTable(
                name: "almacenes");
        }
    }
}

