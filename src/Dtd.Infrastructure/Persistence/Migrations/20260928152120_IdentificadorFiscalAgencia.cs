using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentificadorFiscalAgencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tax_id",
                table: "agencia_bases");

            migrationBuilder.AddColumn<string>(
                name: "identificador_fiscal",
                table: "agencias",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identificador_fiscal",
                table: "agencias");

            migrationBuilder.AddColumn<string>(
                name: "tax_id",
                table: "agencia_bases",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }
    }
}
