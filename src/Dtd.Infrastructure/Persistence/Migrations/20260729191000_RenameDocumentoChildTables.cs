using Dtd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DtdDbContext))]
    [Migration("20260729191000_RenameDocumentoChildTables")]
    public partial class RenameDocumentoChildTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ccs_documento",
                newName: "documento_ccs");

            migrationBuilder.RenameTable(
                name: "conductores_documento",
                newName: "documento_conductores");

            migrationBuilder.RenameTable(
                name: "envios",
                newName: "documento_envios");

            migrationBuilder.RenameTable(
                name: "expediciones",
                newName: "documento_expediciones");

            migrationBuilder.RenameIndex(
                name: "ix_ccs_documento_documento_id_cc_catalog_id",
                table: "documento_ccs",
                newName: "ix_documento_ccs_documento_id_cc_catalog_id");

            migrationBuilder.RenameIndex(
                name: "ix_conductores_documento_documento_id_conductor_catalog_id",
                table: "documento_conductores",
                newName: "ix_documento_conductores_documento_id_conductor_catalog_id");

            migrationBuilder.RenameIndex(
                name: "ix_envios_documento_id",
                table: "documento_envios",
                newName: "ix_documento_envios_documento_id");

            migrationBuilder.RenameIndex(
                name: "ix_expediciones_agencia_id",
                table: "documento_expediciones",
                newName: "ix_documento_expediciones_agencia_id");

            migrationBuilder.RenameIndex(
                name: "ix_expediciones_almacen_id",
                table: "documento_expediciones",
                newName: "ix_documento_expediciones_almacen_id");

            migrationBuilder.RenameIndex(
                name: "ix_expediciones_documento_id",
                table: "documento_expediciones",
                newName: "ix_documento_expediciones_documento_id");

            migrationBuilder.RenameIndex(
                name: "ix_expediciones_envio_id",
                table: "documento_expediciones",
                newName: "ix_documento_expediciones_envio_id");

            migrationBuilder.RenameIndex(
                name: "ix_expediciones_empresa_almacen_id_agencia_id_erp_id",
                table: "documento_expediciones",
                newName: "ix_documento_expediciones_empresa_almacen_id_agencia_id_erp_id");

            RenameConstraintsUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RenameConstraintsDown(migrationBuilder);

            migrationBuilder.RenameIndex(
                name: "ix_documento_ccs_documento_id_cc_catalog_id",
                table: "documento_ccs",
                newName: "ix_ccs_documento_documento_id_cc_catalog_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_conductores_documento_id_conductor_catalog_id",
                table: "documento_conductores",
                newName: "ix_conductores_documento_documento_id_conductor_catalog_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_envios_documento_id",
                table: "documento_envios",
                newName: "ix_envios_documento_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_expediciones_agencia_id",
                table: "documento_expediciones",
                newName: "ix_expediciones_agencia_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_expediciones_almacen_id",
                table: "documento_expediciones",
                newName: "ix_expediciones_almacen_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_expediciones_documento_id",
                table: "documento_expediciones",
                newName: "ix_expediciones_documento_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_expediciones_envio_id",
                table: "documento_expediciones",
                newName: "ix_expediciones_envio_id");

            migrationBuilder.RenameIndex(
                name: "ix_documento_expediciones_empresa_almacen_id_agencia_id_erp_id",
                table: "documento_expediciones",
                newName: "ix_expediciones_empresa_almacen_id_agencia_id_erp_id");

            migrationBuilder.RenameTable(
                name: "documento_ccs",
                newName: "ccs_documento");

            migrationBuilder.RenameTable(
                name: "documento_conductores",
                newName: "conductores_documento");

            migrationBuilder.RenameTable(
                name: "documento_envios",
                newName: "envios");

            migrationBuilder.RenameTable(
                name: "documento_expediciones",
                newName: "expediciones");
        }

        private static void RenameConstraintsUp(MigrationBuilder migrationBuilder)
        {
            RenameConstraintIfExists(migrationBuilder, "documento_ccs", "pk_ccs_documento", "pk_documento_ccs");
            RenameConstraintIfExists(migrationBuilder, "documento_ccs", "fk_ccs_documento_documentos_documento_id", "fk_documento_ccs_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_ccs", "fk_ccs_documento_ccs_documento_id", "fk_documento_ccs_documento_ccs_id");

            RenameConstraintIfExists(migrationBuilder, "documento_conductores", "pk_conductores_documento", "pk_documento_conductores");
            RenameConstraintIfExists(migrationBuilder, "documento_conductores", "fk_conductores_documento_documentos_documento_id", "fk_documento_conductores_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_conductores", "fk_conductores_documento_conductores_documento_id", "fk_documento_conductores_documento_conductores_id");

            RenameConstraintIfExists(migrationBuilder, "documento_envios", "pk_envios", "pk_documento_envios");
            RenameConstraintIfExists(migrationBuilder, "documento_envios", "fk_envios_documentos_documento_id", "fk_documento_envios_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_envios", "fk_envios_envios_id", "fk_documento_envios_documento_envios_id");

            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "pk_expediciones", "pk_documento_expediciones");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_expediciones_agencias_agencia_id", "fk_documento_expediciones_agencias_agencia_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_expediciones_almacenes_almacen_id", "fk_documento_expediciones_almacenes_almacen_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_expediciones_documentos_documento_id", "fk_documento_expediciones_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_expediciones_envios_envio_id", "fk_documento_expediciones_documento_envios_envio_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_expediciones_expediciones_id", "fk_documento_expediciones_documento_expediciones_id");
        }

        private static void RenameConstraintsDown(MigrationBuilder migrationBuilder)
        {
            RenameConstraintIfExists(migrationBuilder, "documento_ccs", "pk_documento_ccs", "pk_ccs_documento");
            RenameConstraintIfExists(migrationBuilder, "documento_ccs", "fk_documento_ccs_documentos_documento_id", "fk_ccs_documento_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_ccs", "fk_documento_ccs_documento_ccs_id", "fk_ccs_documento_ccs_documento_id");

            RenameConstraintIfExists(migrationBuilder, "documento_conductores", "pk_documento_conductores", "pk_conductores_documento");
            RenameConstraintIfExists(migrationBuilder, "documento_conductores", "fk_documento_conductores_documentos_documento_id", "fk_conductores_documento_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_conductores", "fk_documento_conductores_documento_conductores_id", "fk_conductores_documento_conductores_documento_id");

            RenameConstraintIfExists(migrationBuilder, "documento_envios", "pk_documento_envios", "pk_envios");
            RenameConstraintIfExists(migrationBuilder, "documento_envios", "fk_documento_envios_documentos_documento_id", "fk_envios_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_envios", "fk_documento_envios_documento_envios_id", "fk_envios_envios_id");

            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "pk_documento_expediciones", "pk_expediciones");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_documento_expediciones_agencias_agencia_id", "fk_expediciones_agencias_agencia_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_documento_expediciones_almacenes_almacen_id", "fk_expediciones_almacenes_almacen_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_documento_expediciones_documentos_documento_id", "fk_expediciones_documentos_documento_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_documento_expediciones_documento_envios_envio_id", "fk_expediciones_envios_envio_id");
            RenameConstraintIfExists(migrationBuilder, "documento_expediciones", "fk_documento_expediciones_documento_expediciones_id", "fk_expediciones_expediciones_id");
        }

        private static void RenameConstraintIfExists(
            MigrationBuilder migrationBuilder,
            string table,
            string oldName,
            string newName)
        {
            migrationBuilder.Sql($"""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = current_schema()
                            AND t.relname = '{table}'
                            AND c.conname = '{oldName}'
                    ) THEN
                        ALTER TABLE {table} RENAME CONSTRAINT {oldName} TO {newName};
                    END IF;
                END $$;
                """);
        }
    }
}
