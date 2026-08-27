using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoFinalizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "finalizado",
                table: "documentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "finalizado",
                table: "documentos");
        }
    }
}
