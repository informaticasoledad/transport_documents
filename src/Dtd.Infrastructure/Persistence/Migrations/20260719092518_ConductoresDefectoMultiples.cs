using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConductoresDefectoMultiples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Crea la nueva tabla de defaults (varios por tupla) antes de tocar la vieja,
            //    para poder migrar los defaults existentes sin perderlos.
            migrationBuilder.CreateTable(
                name: "almacen_agencia_conductores_defecto",
                columns: table => new
                {
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conductor_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_almacen_agencia_conductores_defecto", x => new { x.almacen_id, x.agencia_id, x.conductor_id });
                    table.ForeignKey(
                        name: "fk_almacen_agencia_conductores_defecto_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_conductores_defecto_almacenes_almacen_id",
                        column: x => x.almacen_id,
                        principalTable: "almacenes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_almacen_agencia_conductores_defecto_conductores_conductor_id",
                        column: x => x.conductor_id,
                        principalTable: "conductores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_conductores_defecto_agencia_id",
                table: "almacen_agencia_conductores_defecto",
                column: "agencia_id");

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencia_conductores_defecto_conductor_id",
                table: "almacen_agencia_conductores_defecto",
                column: "conductor_id");

            // 2) Migra los defaults existentes (columna única) a la nueva tabla (varios).
            migrationBuilder.Sql(
                """
                INSERT INTO almacen_agencia_conductores_defecto (almacen_id, agencia_id, conductor_id)
                SELECT almacen_id, agencia_id, conductor_defecto_id
                FROM almacen_agencias
                WHERE conductor_defecto_id IS NOT NULL
                """);

            // 3) Elimina la columna única vieja y sus restricciones.
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencias_conductores_conductor_defecto_id",
                table: "almacen_agencias");

            migrationBuilder.DropIndex(
                name: "ix_almacen_agencias_conductor_defecto_id",
                table: "almacen_agencias");

            migrationBuilder.DropColumn(
                name: "conductor_defecto_id",
                table: "almacen_agencias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaura la columna única (un default por tupla; si había varios, se queda uno
            // arbitrario — es un downgrade de dev) y copia desde la nueva tabla antes de borrarla.
            migrationBuilder.AddColumn<Guid>(
                name: "conductor_defecto_id",
                table: "almacen_agencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE almacen_agencias a
                SET conductor_defecto_id = d.conductor_id
                FROM (
                    SELECT DISTINCT ON (almacen_id, agencia_id) almacen_id, agencia_id, conductor_id
                    FROM almacen_agencia_conductores_defecto
                    ORDER BY almacen_id, agencia_id, conductor_id
                ) d
                WHERE a.almacen_id = d.almacen_id AND a.agencia_id = d.agencia_id
                """);

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencias_conductor_defecto_id",
                table: "almacen_agencias",
                column: "conductor_defecto_id");

            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencias_conductores_conductor_defecto_id",
                table: "almacen_agencias",
                column: "conductor_defecto_id",
                principalTable: "conductores",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropTable(
                name: "almacen_agencia_conductores_defecto");
        }
    }
}
