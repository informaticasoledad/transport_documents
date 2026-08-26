using System;
using Dtd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DtdDbContext))]
    [Migration("20260730211500_CcsPorRelacionAlmacenAgencia")]
    public partial class CcsPorRelacionAlmacenAgencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "almacen_agencia_ccs",
                columns: table => new
                {
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    por_defecto = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacen_agencia_ccs", x => new { x.almacen_id, x.agencia_id, x.cc_id });
                    table.ForeignKey(
                        name: "fk_almacen_agencia_ccs_almacen_agencias",
                        columns: x => new { x.almacen_id, x.agencia_id },
                        principalTable: "almacen_agencias",
                        principalColumns: new[] { "almacen_id", "agencia_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_ccs_ccs",
                        column: x => x.cc_id,
                        principalTable: "ccs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO almacen_agencia_ccs (almacen_id, agencia_id, cc_id, por_defecto)
                SELECT DISTINCT
                    ca.almacen_id,
                    cag.agencia_id,
                    ca.cc_id,
                    EXISTS (
                        SELECT 1
                        FROM almacen_agencia_ccs_defecto d
                        WHERE d.almacen_id = ca.almacen_id
                          AND d.agencia_id = cag.agencia_id
                          AND d.cc_id = ca.cc_id
                    ) AS por_defecto
                FROM cc_almacenes ca
                INNER JOIN cc_agencias cag ON cag.cc_id = ca.cc_id
                INNER JOIN almacen_agencias aa
                    ON aa.almacen_id = ca.almacen_id
                   AND aa.agencia_id = cag.agencia_id
                ON CONFLICT (almacen_id, agencia_id, cc_id)
                DO UPDATE SET por_defecto = almacen_agencia_ccs.por_defecto OR EXCLUDED.por_defecto;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_ccs_cc_id",
                table: "almacen_agencia_ccs",
                column: "cc_id");

            migrationBuilder.DropTable(
                name: "almacen_agencia_ccs_defecto");

            migrationBuilder.DropTable(
                name: "cc_agencias");

            migrationBuilder.DropTable(
                name: "cc_almacenes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(
                """
                INSERT INTO cc_almacenes (cc_id, almacen_id)
                SELECT DISTINCT cc_id, almacen_id
                FROM almacen_agencia_ccs;

                INSERT INTO cc_agencias (cc_id, agencia_id)
                SELECT DISTINCT cc_id, agencia_id
                FROM almacen_agencia_ccs;

                INSERT INTO almacen_agencia_ccs_defecto (almacen_id, agencia_id, cc_id)
                SELECT almacen_id, agencia_id, cc_id
                FROM almacen_agencia_ccs
                WHERE por_defecto;
                """);

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

            migrationBuilder.DropTable(
                name: "almacen_agencia_ccs");
        }
    }
}
