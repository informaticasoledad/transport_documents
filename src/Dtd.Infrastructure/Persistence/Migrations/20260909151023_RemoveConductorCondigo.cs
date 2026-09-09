using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConductorCondigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_conductores_codigo",
                table: "conductores");

            migrationBuilder.DropColumn(
                name: "conductor_codigo",
                table: "documento_conductores");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "conductores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conductor_codigo",
                table: "documento_conductores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "conductores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_conductores_codigo",
                table: "conductores",
                column: "codigo",
                unique: true);
        }
    }
}
