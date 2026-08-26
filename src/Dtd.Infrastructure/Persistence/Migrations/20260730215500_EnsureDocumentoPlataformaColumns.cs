using Dtd.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DtdDbContext))]
    [Migration("20260730215500_EnsureDocumentoPlataformaColumns")]
    public partial class EnsureDocumentoPlataformaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE documentos
                ADD COLUMN IF NOT EXISTS plataforma_id character varying(100);

                ALTER TABLE documentos
                ADD COLUMN IF NOT EXISTS plataforma_estado character varying(50);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE documentos
                DROP COLUMN IF EXISTS plataforma_id;

                ALTER TABLE documentos
                DROP COLUMN IF EXISTS plataforma_estado;
                """);
        }
    }
}
