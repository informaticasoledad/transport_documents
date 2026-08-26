using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlmacenAgenciaIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Añade las nuevas columnas FK como NULLABLE para poder backfillear desde los códigos
            //    existentes sin perder la relación. EF generaba NOT NULL + default Guid.Empty, que
            //    rompería con datos existentes; se edita a mano la secuencia
            //    nullable → diagnóstico → backfill → SET NOT NULL → FK → drop → índices.
            migrationBuilder.AddColumn<Guid>(
                name: "almacen_id",
                table: "documentos",
                type: "uuid",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "agencia_id",
                table: "documentos",
                type: "uuid",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "almacen_id",
                table: "expediciones",
                type: "uuid",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "agencia_id",
                table: "expediciones",
                type: "uuid",
                nullable: true);

            // 2) Diagnóstico pre-vuelo (fail-and-list): si hay documentos/expediciones cuyo
            //    (empresa, almacen_codigo)/(empresa, agencia_codigo) ya no existe en el maestro
            //    local, aborta la migración listando las tuplas huérfanas antes del SET NOT NULL.
            //    Así dev obtiene un error claro en vez de un críptico de NOT NULL/FK de Postgres.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM documentos d
                    LEFT JOIN almacenes a ON a.empresa = d.empresa AND a.codigo = d.almacen_codigo
                    WHERE d.almacen_codigo IS NOT NULL AND a.id IS NULL
                  ) THEN
                    RAISE EXCEPTION 'Documentos huérfanos (almacen_codigo sin maestro): %',
                      (SELECT string_agg(d.empresa || ':' || d.almacen_codigo, ', ')
                         FROM documentos d
                         LEFT JOIN almacenes a ON a.empresa = d.empresa AND a.codigo = d.almacen_codigo
                        WHERE d.almacen_codigo IS NOT NULL AND a.id IS NULL);
                  END IF;
                  IF EXISTS (
                    SELECT 1 FROM documentos d
                    LEFT JOIN agencias g ON g.empresa = d.empresa AND g.codigo = d.agencia_codigo
                    WHERE d.agencia_codigo IS NOT NULL AND g.id IS NULL
                  ) THEN
                    RAISE EXCEPTION 'Documentos huérfanos (agencia_codigo sin maestro): %',
                      (SELECT string_agg(d.empresa || ':' || d.agencia_codigo, ', ')
                         FROM documentos d
                         LEFT JOIN agencias g ON g.empresa = d.empresa AND g.codigo = d.agencia_codigo
                        WHERE d.agencia_codigo IS NOT NULL AND g.id IS NULL);
                  END IF;
                  IF EXISTS (
                    SELECT 1 FROM expediciones e
                    LEFT JOIN almacenes a ON a.empresa = e.empresa AND a.codigo = e.almacen_codigo
                    WHERE e.almacen_codigo IS NOT NULL AND a.id IS NULL
                  ) THEN
                    RAISE EXCEPTION 'Expediciones huérfanas (almacen_codigo sin maestro): %',
                      (SELECT string_agg(e.empresa || ':' || e.almacen_codigo, ', ')
                         FROM expediciones e
                         LEFT JOIN almacenes a ON a.empresa = e.empresa AND a.codigo = e.almacen_codigo
                        WHERE e.almacen_codigo IS NOT NULL AND a.id IS NULL);
                  END IF;
                  IF EXISTS (
                    SELECT 1 FROM expediciones e
                    LEFT JOIN agencias g ON g.empresa = e.empresa AND g.codigo = e.agencia_codigo
                    WHERE e.agencia_codigo IS NOT NULL AND g.id IS NULL
                  ) THEN
                    RAISE EXCEPTION 'Expediciones huérfanas (agencia_codigo sin maestro): %',
                      (SELECT string_agg(e.empresa || ':' || e.agencia_codigo, ', ')
                         FROM expediciones e
                         LEFT JOIN agencias g ON g.empresa = e.empresa AND g.codigo = e.agencia_codigo
                        WHERE e.agencia_codigo IS NOT NULL AND g.id IS NULL);
                  END IF;
                END $$;
                """);

            // 3) Backfill: resuelve el Id del maestro por (empresa, codigo). Las columnas viejas
            //    eran NOT NULL, así que toda fila tiene código → toda fila queda con Id.
            migrationBuilder.Sql("""
                UPDATE documentos d
                SET almacen_id = a.id
                FROM almacenes a
                WHERE a.empresa = d.empresa AND a.codigo = d.almacen_codigo;

                UPDATE documentos d
                SET agencia_id = g.id
                FROM agencias g
                WHERE g.empresa = d.empresa AND g.codigo = d.agencia_codigo;

                UPDATE expediciones e
                SET almacen_id = a.id
                FROM almacenes a
                WHERE a.empresa = e.empresa AND a.codigo = e.almacen_codigo;

                UPDATE expediciones e
                SET agencia_id = g.id
                FROM agencias g
                WHERE g.empresa = e.empresa AND g.codigo = e.agencia_codigo;
                """);

            // 4) SET NOT NULL: tras el backfill todas las filas tienen Id.
            migrationBuilder.Sql("""
                ALTER TABLE documentos ALTER COLUMN almacen_id SET NOT NULL;
                ALTER TABLE documentos ALTER COLUMN agencia_id SET NOT NULL;
                ALTER TABLE expediciones ALTER COLUMN almacen_id SET NOT NULL;
                ALTER TABLE expediciones ALTER COLUMN agencia_id SET NOT NULL;
                """);

            // 5) FKs RESTRICT: a partir de ahora no se puede borrar un almacén/agencia con
            //    documentos/expediciones (refuerza integridad; sustituye al objetivo previo de
            //    "sobrevivir al borrado"). No choca con el cascade existente expediciones→documentos.
            migrationBuilder.AddForeignKey(
                name: "fk_documentos_almacenes_almacen_id",
                table: "documentos",
                column: "almacen_id",
                principalTable: "almacenes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "fk_documentos_agencias_agencia_id",
                table: "documentos",
                column: "agencia_id",
                principalTable: "agencias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "fk_expediciones_almacenes_almacen_id",
                table: "expediciones",
                column: "almacen_id",
                principalTable: "almacenes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "fk_expediciones_agencias_agencia_id",
                table: "expediciones",
                column: "agencia_id",
                principalTable: "agencias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // 6) Drop índices viejos (redundante — Postgres los cae al borrar columna — pero
            //    explícito por claridad).
            migrationBuilder.DropIndex(
                name: "ix_expediciones_empresa_almacen_codigo_agencia_codigo_erp_id",
                table: "expediciones");
            migrationBuilder.DropIndex(
                name: "ix_documentos_empresa_almacen_codigo_agencia_codigo",
                table: "documentos");

            // 7) Drop columnas viejas de código.
            migrationBuilder.DropColumn(name: "almacen_codigo", table: "documentos");
            migrationBuilder.DropColumn(name: "agencia_codigo", table: "documentos");
            migrationBuilder.DropColumn(name: "almacen_codigo", table: "expediciones");
            migrationBuilder.DropColumn(name: "agencia_codigo", table: "expediciones");

            // 8) Índices nuevos.
            migrationBuilder.CreateIndex(
                name: "ix_documentos_almacen_id",
                table: "documentos",
                column: "almacen_id");
            migrationBuilder.CreateIndex(
                name: "ix_documentos_agencia_id",
                table: "documentos",
                column: "agencia_id");
            migrationBuilder.CreateIndex(
                name: "ix_documentos_empresa_almacen_id_agencia_id",
                table: "documentos",
                columns: new[] { "empresa", "almacen_id", "agencia_id" });
            migrationBuilder.CreateIndex(
                name: "ix_expediciones_almacen_id",
                table: "expediciones",
                column: "almacen_id");
            migrationBuilder.CreateIndex(
                name: "ix_expediciones_agencia_id",
                table: "expediciones",
                column: "agencia_id");
            migrationBuilder.CreateIndex(
                name: "ix_expediciones_empresa_almacen_id_agencia_id_erp_id",
                table: "expediciones",
                columns: new[] { "empresa", "almacen_id", "agencia_id", "erp_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: drop índices nuevos → drop FKs → restaura cols de código NULL → backfill
            // códigos desde Ids → SET NOT NULL → drop cols de Id → recrea índices viejos.
            migrationBuilder.DropIndex(
                name: "ix_expediciones_empresa_almacen_id_agencia_id_erp_id",
                table: "expediciones");
            migrationBuilder.DropIndex(
                name: "ix_expediciones_agencia_id",
                table: "expediciones");
            migrationBuilder.DropIndex(
                name: "ix_expediciones_almacen_id",
                table: "expediciones");
            migrationBuilder.DropIndex(
                name: "ix_documentos_empresa_almacen_id_agencia_id",
                table: "documentos");
            migrationBuilder.DropIndex(
                name: "ix_documentos_agencia_id",
                table: "documentos");
            migrationBuilder.DropIndex(
                name: "ix_documentos_almacen_id",
                table: "documentos");

            migrationBuilder.DropForeignKey(
                name: "fk_expediciones_agencias_agencia_id",
                table: "expediciones");
            migrationBuilder.DropForeignKey(
                name: "fk_expediciones_almacenes_almacen_id",
                table: "expediciones");
            migrationBuilder.DropForeignKey(
                name: "fk_documentos_agencias_agencia_id",
                table: "documentos");
            migrationBuilder.DropForeignKey(
                name: "fk_documentos_almacenes_almacen_id",
                table: "documentos");

            // Restaura las columnas de código como NULLABLE y backfillea desde los Ids.
            migrationBuilder.AddColumn<string>(
                name: "almacen_codigo",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "agencia_codigo",
                table: "documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "almacen_codigo",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "agencia_codigo",
                table: "expediciones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE documentos d SET almacen_codigo = a.codigo
                FROM almacenes a WHERE d.almacen_id = a.id;
                UPDATE documentos d SET agencia_codigo = g.codigo
                FROM agencias g WHERE d.agencia_id = g.id;
                UPDATE expediciones e SET almacen_codigo = a.codigo
                FROM almacenes a WHERE e.almacen_id = a.id;
                UPDATE expediciones e SET agencia_codigo = g.codigo
                FROM agencias g WHERE e.agencia_id = g.id;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE documentos ALTER COLUMN almacen_codigo SET NOT NULL;
                ALTER TABLE documentos ALTER COLUMN agencia_codigo SET NOT NULL;
                ALTER TABLE expediciones ALTER COLUMN almacen_codigo SET NOT NULL;
                ALTER TABLE expediciones ALTER COLUMN agencia_codigo SET NOT NULL;
                """);

            migrationBuilder.DropColumn(name: "almacen_id", table: "documentos");
            migrationBuilder.DropColumn(name: "agencia_id", table: "documentos");
            migrationBuilder.DropColumn(name: "almacen_id", table: "expediciones");
            migrationBuilder.DropColumn(name: "agencia_id", table: "expediciones");

            migrationBuilder.CreateIndex(
                name: "ix_documentos_empresa_almacen_codigo_agencia_codigo",
                table: "documentos",
                columns: new[] { "empresa", "almacen_codigo", "agencia_codigo" });
            migrationBuilder.CreateIndex(
                name: "ix_expediciones_empresa_almacen_codigo_agencia_codigo_erp_id",
                table: "expediciones",
                columns: new[] { "empresa", "almacen_codigo", "agencia_codigo", "erp_id" },
                unique: true);
        }
    }
}