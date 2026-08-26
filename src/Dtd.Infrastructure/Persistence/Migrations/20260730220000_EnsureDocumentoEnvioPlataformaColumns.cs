using Dtd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DtdDbContext))]
    [Migration("20260730220000_EnsureDocumentoEnvioPlataformaColumns")]
    public partial class EnsureDocumentoEnvioPlataformaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'docuten_id'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'plataforma_envio_id'
                    ) THEN
                        ALTER TABLE documento_envios
                        RENAME COLUMN docuten_id TO plataforma_envio_id;
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'docuten_estado'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'plataforma_envio_estado'
                    ) THEN
                        ALTER TABLE documento_envios
                        RENAME COLUMN docuten_estado TO plataforma_envio_estado;
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'plataforma_id'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'plataforma_envio_id'
                    ) THEN
                        ALTER TABLE documento_envios
                        RENAME COLUMN plataforma_id TO plataforma_envio_id;
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'plataforma_estado'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'documento_envios'
                          AND column_name = 'plataforma_envio_estado'
                    ) THEN
                        ALTER TABLE documento_envios
                        RENAME COLUMN plataforma_estado TO plataforma_envio_estado;
                    END IF;
                END $$;

                ALTER TABLE documento_envios
                ADD COLUMN IF NOT EXISTS plataforma_envio_id character varying(100);

                ALTER TABLE documento_envios
                ADD COLUMN IF NOT EXISTS plataforma_envio_estado character varying(50);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE documento_envios
                DROP COLUMN IF EXISTS plataforma_envio_id;

                ALTER TABLE documento_envios
                DROP COLUMN IF EXISTS plataforma_envio_estado;
                """);
        }
    }
}
