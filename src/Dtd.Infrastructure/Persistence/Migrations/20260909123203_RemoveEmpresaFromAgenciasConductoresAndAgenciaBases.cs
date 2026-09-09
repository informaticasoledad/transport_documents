using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmpresaFromAgenciasConductoresAndAgenciaBases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_conductores_empresa_codigo",
                table: "conductores");

            migrationBuilder.DropIndex(
                name: "ix_agencias_empresa_codigo",
                table: "agencias");

            migrationBuilder.DropIndex(
                name: "ix_agencia_bases_empresa_codigo",
                table: "agencia_bases");

            // 1. Añadimos primero AgenciaId nullable temporalmente.
            migrationBuilder.AddColumn<Guid>(
                name: "agencia_id",
                table: "agencia_bases",
                type: "uuid",
                nullable: true);

            // 2. Backfill aquí.
            //
            // NECESITAMOS definir cómo sabemos a qué agencia
            // pertenece cada agencia_base existente.
            //
            // Ejemplo, si el código de la base contiene/prefija
            // el código de agencia:
            migrationBuilder.Sql("""
        UPDATE agencia_bases ab
        SET agencia_id = a.id
        FROM agencias a
        WHERE ab.codigo LIKE a.codigo || '%'
          AND ab.empresa = a.empresa;
        """);

            // 3. Fallamos explícitamente si quedó alguna sin resolver.
            migrationBuilder.Sql("""
        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1
                FROM agencia_bases
                WHERE agencia_id IS NULL
            ) THEN
                RAISE EXCEPTION
                    'Existen agencia_bases sin agencia asociada';
            END IF;
        END $$;
        """);

            // 4. Ahora sí hacemos la columna obligatoria.
            migrationBuilder.AlterColumn<Guid>(
                name: "agencia_id",
                table: "agencia_bases",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 5. Ya podemos eliminar Empresa.
            migrationBuilder.DropColumn(
                name: "empresa",
                table: "conductores");

            migrationBuilder.DropColumn(
                name: "empresa",
                table: "agencias");

            migrationBuilder.DropColumn(
                name: "empresa",
                table: "agencia_bases");

            migrationBuilder.CreateIndex(
                name: "ix_conductores_codigo",
                table: "conductores",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agencias_codigo",
                table: "agencias",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agencia_bases_agencia_id_codigo",
                table: "agencia_bases",
                columns: new[] { "agencia_id", "codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_agencia_bases_agencias_agencia_id",
                table: "agencia_bases",
                column: "agencia_id",
                principalTable: "agencias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agencia_bases_agencias_agencia_id",
                table: "agencia_bases");

            migrationBuilder.DropIndex(
                name: "ix_conductores_codigo",
                table: "conductores");

            migrationBuilder.DropIndex(
                name: "ix_agencias_codigo",
                table: "agencias");

            migrationBuilder.DropIndex(
                name: "ix_agencia_bases_agencia_id_codigo",
                table: "agencia_bases");

            migrationBuilder.DropColumn(
                name: "agencia_id",
                table: "agencia_bases");

            migrationBuilder.AddColumn<string>(
                name: "empresa",
                table: "conductores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "empresa",
                table: "agencias",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "empresa",
                table: "agencia_bases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_conductores_empresa_codigo",
                table: "conductores",
                columns: new[] { "empresa", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agencias_empresa_codigo",
                table: "agencias",
                columns: new[] { "empresa", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agencia_bases_empresa_codigo",
                table: "agencia_bases",
                columns: new[] { "empresa", "codigo" },
                unique: true);
        }
    }
}
