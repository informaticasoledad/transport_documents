using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgenciaBaseClientCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agencia_bases_agencias_agencia_id",
                table: "agencia_bases");

            migrationBuilder.AddForeignKey(
                name: "fk_agencia_bases_agencias_agencia_id",
                table: "agencia_bases",
                column: "agencia_id",
                principalTable: "agencias",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agencia_bases_agencias_agencia_id",
                table: "agencia_bases");

            migrationBuilder.AddForeignKey(
                name: "fk_agencia_bases_agencias_agencia_id",
                table: "agencia_bases",
                column: "agencia_id",
                principalTable: "agencias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
