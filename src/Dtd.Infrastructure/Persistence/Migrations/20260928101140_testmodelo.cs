using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class testmodelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "almacen_agencia_bases_defecto");

            migrationBuilder.DropColumn(
                name: "channel",
                table: "agencia_bases");

            migrationBuilder.DropColumn(
                name: "movil",
                table: "agencia_bases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "channel",
                table: "agencia_bases",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "movil",
                table: "agencia_bases",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "almacen_agencia_bases_defecto",
                columns: table => new
                {
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_base_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacen_agencia_bases_defecto", x => new { x.almacen_id, x.agencia_id, x.agencia_base_id });
                    table.ForeignKey(
                        name: "fk_almacen_agencia_bases_defecto_agencia_bases",
                        column: x => x.agencia_base_id,
                        principalTable: "agencia_bases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_bases_defecto_agencias",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_bases_defecto_almacenes",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_bases_defecto_agencia_base_id",
                table: "almacen_agencia_bases_defecto",
                column: "agencia_base_id");

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_bases_defecto_agencia_id",
                table: "almacen_agencia_bases_defecto",
                column: "agencia_id");
        }
    }
}
