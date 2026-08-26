using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Renombrado limpio de la tabla <c>enterprises</c> ? <c>empresas</c> (consistencia con el resto del
    /// esquema en español). Se hace con <c>RenameTable</c> en vez de drop+create para conservar los datos.
    /// El constraint PK se renombra explícitamente porque PostgreSQL no lo hace automáticamente al
    /// renombrar la tabla (<c>pk_enterprises</c> ? <c>pk_empresas</c>), igualando el nombre que espera el
    /// modelo y evitando drift en la siguiente diff. La columna <c>empresa</c> no cambia de nombre.
    /// </remarks>
    public partial class RenameEnterprisesToEmpresas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "enterprises", newName: "empresas");

            // PostgreSQL no renombra el constraint al renombrar la tabla; lo hacemos a mano.
            migrationBuilder.Sql("ALTER TABLE empresas RENAME CONSTRAINT pk_enterprises TO pk_empresas;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE empresas RENAME CONSTRAINT pk_empresas TO pk_enterprises;");

            migrationBuilder.RenameTable(name: "empresas", newName: "enterprises");
        }
    }
}
