using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConductoresMuchosAMuchos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Añade `empresa` (3 dígitos) con default '' y la backfill-ea desde la agencia a la
            //    que pertenece cada conductor, ANTES de soltar `agencia_id`.
            migrationBuilder.AddColumn<string>(
                name: "empresa",
                table: "conductores",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE conductores c SET empresa = a.empresa FROM agencias a WHERE c.agencia_id = a.id;");

            // 2) Crea la tabla join `conductor_agencias` (M:N) y la puebla desde el 1:N existente:
            //    cada fila (id, agencia_id) de `conductores` se convierte en el primer vínculo M:N,
            //    preservando así la relación anterior.
            migrationBuilder.CreateTable(
                name: "conductor_agencias",
                columns: table => new
                {
                    conductor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agencia_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conductor_agencias", x => new { x.conductor_id, x.agencia_id });
                    table.ForeignKey(
                        name: "fk_conductor_agencias_agencias_agencia_id",
                        column: x => x.agencia_id,
                        principalTable: "agencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conductor_agencias_conductores_conductor_id",
                        column: x => x.conductor_id,
                        principalTable: "conductores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                "INSERT INTO conductor_agencias (conductor_id, agencia_id) SELECT id, agencia_id FROM conductores;");

            // 3) Suelta la FK y el índice único (agencia_id, codigo), luego la columna `agencia_id`
            //    (la relación M:N ya vive en conductor_agencias).
            migrationBuilder.DropForeignKey(
                name: "fk_conductores_agencias_agencia_id",
                table: "conductores");

            migrationBuilder.DropIndex(
                name: "ix_conductores_agencia_id_codigo",
                table: "conductores");

            migrationBuilder.DropColumn(
                name: "agencia_id",
                table: "conductores");

            // 4) Único (empresa, codigo): un conductor se identifica por su empresa + código (puede
            //    servir a varias agencias, pero todas de la misma empresa).
            migrationBuilder.CreateIndex(
                name: "ix_conductores_empresa_codigo",
                table: "conductores",
                columns: new[] { "empresa", "codigo" },
                unique: true);

            // 5) Snapshot del Id de catálogo en conductores_documento (nueva clave de idempotencia).
            //    Backfill-ea desde el catálogo por (codigo, empresa del documento). Sin ambigüedad:
            //    (empresa, codigo) ya es único en conductores (paso 4), así que (codigo, empresa del
            //    documento) identifica un único conductor de esa empresa. Si existiera un duplicado
            //    histórico, el unique del paso 4 habría abortado la migración antes de llegar aquí.
            //    Nota de sintaxis PostgreSQL: la tabla objetivo (cd) NO se puede referenciar en el
            //    JOIN ON del FROM; su condición de cruce con `documentos` va al WHERE.
            migrationBuilder.AddColumn<Guid>(
                name: "conductor_catalog_id",
                table: "conductores_documento",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                """
                UPDATE conductores_documento cd
                SET conductor_catalog_id = c.id
                FROM conductores c, documentos d
                WHERE cd.documento_id = d.id
                  AND cd.conductor_codigo = c.codigo
                  AND c.empresa = d.empresa;
                """);

            // 6) Sustituye el índice único de conductores_documento: de (documento_id, conductor_codigo)
            //    a (documento_id, conductor_catalog_id). El código sigue como snapshot de display.
            migrationBuilder.DropIndex(
                name: "ix_conductores_documento_documento_id_conductor_codigo",
                table: "conductores_documento");

            migrationBuilder.CreateIndex(
                name: "ix_conductores_documento_documento_id_conductor_catalog_id",
                table: "conductores_documento",
                columns: new[] { "documento_id", "conductor_catalog_id" },
                unique: true);

            // 7) Índice sobre la FK a agencias (la otra mitad de la PK ya indexa conductor_id).
            migrationBuilder.CreateIndex(
                name: "ix_conductor_agencias_agencia_id",
                table: "conductor_agencias",
                column: "agencia_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deshace en orden inverso, repoblando `agencia_id` desde el join M:N (una agencia por
            // conductor, la mínima) antes de soltar la tabla join.
            migrationBuilder.DropIndex(
                name: "ix_conductor_agencias_agencia_id",
                table: "conductor_agencias");

            migrationBuilder.DropIndex(
                name: "ix_conductores_documento_documento_id_conductor_catalog_id",
                table: "conductores_documento");

            migrationBuilder.DropIndex(
                name: "ix_conductores_empresa_codigo",
                table: "conductores");

            migrationBuilder.AddColumn<Guid>(
                name: "agencia_id",
                table: "conductores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                """
                UPDATE conductores c
                SET agencia_id = sub.agencia_id
                FROM (
                    SELECT conductor_id, MIN(agencia_id) AS agencia_id
                    FROM conductor_agencias
                    GROUP BY conductor_id
                ) sub
                WHERE c.id = sub.conductor_id;
                """);

            migrationBuilder.DropTable(
                name: "conductor_agencias");

            migrationBuilder.DropColumn(
                name: "conductor_catalog_id",
                table: "conductores_documento");

            migrationBuilder.DropColumn(
                name: "empresa",
                table: "conductores");

            migrationBuilder.CreateIndex(
                name: "ix_conductores_documento_documento_id_conductor_codigo",
                table: "conductores_documento",
                columns: new[] { "documento_id", "conductor_codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conductores_agencia_id_codigo",
                table: "conductores",
                columns: new[] { "agencia_id", "codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_conductores_agencias_agencia_id",
                table: "conductores",
                column: "agencia_id",
                principalTable: "agencias",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}