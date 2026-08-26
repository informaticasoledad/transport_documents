using Dtd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
[DbContext(typeof(DtdDbContext))]
[Migration("20260730210000_RenameConsigneesToAgenciaBases")]
public partial class RenameConsigneesToAgenciaBases : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        RenameTableIfNeeded(migrationBuilder, "consignees", "agencia_bases");
        RenameColumnIfNeeded(migrationBuilder, "almacen_agencias", "consignee_base_id", "agencia_base_id");
        RenameTableIfNeeded(
            migrationBuilder,
            "almacen_agencia_consignees_defecto",
            "almacen_agencia_bases_defecto");
        RenameColumnIfNeeded(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "consignee_id",
            "agencia_base_id");

        RenameConstraintIfExists(migrationBuilder, "agencia_bases", "pk_consignees", "pk_agencia_bases");
        RenameConstraintIfExists(
            migrationBuilder,
            "agencia_bases",
            "fk_consignees_consignees_id",
            "fk_agencia_bases_agencia_bases_id");

        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencias",
            "fk_almacen_agencias_agencias_agencia_id",
            "fk_almacen_agencias_agencias");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencias",
            "fk_almacen_agencias_almacenes_almacen_id",
            "fk_almacen_agencias_almacenes");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencias",
            "fk_almacen_agencias_consignees_consignee_base_id",
            "fk_almacen_agencias_agencia_bases");

        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "pk_almacen_agencia_consignees_defecto",
            "pk_almacen_agencia_bases_defecto");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "fk_almacen_agencia_consignees_defecto_agencias_agencia_id",
            "fk_almacen_agencia_bases_defecto_agencias");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "fk_almacen_agencia_consignees_defecto_almacenes_almacen_id",
            "fk_almacen_agencia_bases_defecto_almacenes");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "fk_almacen_agencia_consignees_defecto_consignees_consignee_id",
            "fk_almacen_agencia_bases_defecto_agencia_bases");

        RenameIndexIfExists(migrationBuilder, "ix_consignees_empresa_codigo", "ix_agencia_bases_empresa_codigo");
        RenameIndexIfExists(
            migrationBuilder,
            "ix_almacen_agencias_consignee_base_id",
            "ix_almacen_agencias_agencia_base_id");
        RenameIndexIfExists(
            migrationBuilder,
            "ix_almacen_agencia_consignees_defecto_agencia_id",
            "ix_almacen_agencia_bases_defecto_agencia_id");
        RenameIndexIfExists(
            migrationBuilder,
            "ix_almacen_agencia_consignees_defecto_consignee_id",
            "ix_almacen_agencia_bases_defecto_agencia_base_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        RenameIndexIfExists(migrationBuilder, "ix_agencia_bases_empresa_codigo", "ix_consignees_empresa_codigo");
        RenameIndexIfExists(
            migrationBuilder,
            "ix_almacen_agencias_agencia_base_id",
            "ix_almacen_agencias_consignee_base_id");
        RenameIndexIfExists(
            migrationBuilder,
            "ix_almacen_agencia_bases_defecto_agencia_id",
            "ix_almacen_agencia_consignees_defecto_agencia_id");
        RenameIndexIfExists(
            migrationBuilder,
            "ix_almacen_agencia_bases_defecto_agencia_base_id",
            "ix_almacen_agencia_consignees_defecto_consignee_id");

        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "fk_almacen_agencia_bases_defecto_agencia_bases",
            "fk_almacen_agencia_consignees_defecto_consignees_consignee_id");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "fk_almacen_agencia_bases_defecto_almacenes",
            "fk_almacen_agencia_consignees_defecto_almacenes_almacen_id");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "fk_almacen_agencia_bases_defecto_agencias",
            "fk_almacen_agencia_consignees_defecto_agencias_agencia_id");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "pk_almacen_agencia_bases_defecto",
            "pk_almacen_agencia_consignees_defecto");

        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencias",
            "fk_almacen_agencias_agencia_bases",
            "fk_almacen_agencias_consignees_consignee_base_id");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencias",
            "fk_almacen_agencias_almacenes",
            "fk_almacen_agencias_almacenes_almacen_id");
        RenameConstraintIfExists(
            migrationBuilder,
            "almacen_agencias",
            "fk_almacen_agencias_agencias",
            "fk_almacen_agencias_agencias_agencia_id");

        RenameConstraintIfExists(
            migrationBuilder,
            "agencia_bases",
            "fk_agencia_bases_agencia_bases_id",
            "fk_consignees_consignees_id");
        RenameConstraintIfExists(migrationBuilder, "agencia_bases", "pk_agencia_bases", "pk_consignees");

        RenameColumnIfNeeded(migrationBuilder, "almacen_agencia_bases_defecto", "agencia_base_id", "consignee_id");
        RenameTableIfNeeded(
            migrationBuilder,
            "almacen_agencia_bases_defecto",
            "almacen_agencia_consignees_defecto");
        RenameColumnIfNeeded(migrationBuilder, "almacen_agencias", "agencia_base_id", "consignee_base_id");
        RenameTableIfNeeded(migrationBuilder, "agencia_bases", "consignees");
    }

    private static void RenameTableIfNeeded(MigrationBuilder migrationBuilder, string oldName, string newName)
    {
        migrationBuilder.Sql($"""
            DO $$
            BEGIN
                IF to_regclass('public.{oldName}') IS NOT NULL
                   AND to_regclass('public.{newName}') IS NULL THEN
                    ALTER TABLE public."{oldName}" RENAME TO "{newName}";
                END IF;
            END $$;
            """);
    }

    private static void RenameColumnIfNeeded(
        MigrationBuilder migrationBuilder,
        string tableName,
        string oldName,
        string newName)
    {
        migrationBuilder.Sql($"""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = '{tableName}'
                      AND column_name = '{oldName}'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = '{tableName}'
                      AND column_name = '{newName}'
                ) THEN
                    ALTER TABLE public."{tableName}" RENAME COLUMN "{oldName}" TO "{newName}";
                END IF;
            END $$;
            """);
    }

    private static void RenameConstraintIfExists(
        MigrationBuilder migrationBuilder,
        string tableName,
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
                    WHERE n.nspname = 'public'
                      AND t.relname = '{tableName}'
                      AND c.conname = '{oldName}'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = '{newName}'
                ) THEN
                    ALTER TABLE public."{tableName}" RENAME CONSTRAINT "{oldName}" TO "{newName}";
                END IF;
            END $$;
            """);
    }

    private static void RenameIndexIfExists(MigrationBuilder migrationBuilder, string oldName, string newName)
    {
        migrationBuilder.Sql($"""
            DO $$
            BEGIN
                IF to_regclass('public.{oldName}') IS NOT NULL
                   AND to_regclass('public.{newName}') IS NULL THEN
                    ALTER INDEX public."{oldName}" RENAME TO "{newName}";
                END IF;
            END $$;
            """);
    }
}
