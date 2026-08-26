using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveErpOauthConfigToAppsettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_id",
                table: "enterprises");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "enterprises");

            migrationBuilder.DropColumn(
                name: "timeout_seconds",
                table: "enterprises");

            migrationBuilder.DropColumn(
                name: "token_endpoint",
                table: "enterprises");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_id",
                table: "enterprises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "enterprises",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "timeout_seconds",
                table: "enterprises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_endpoint",
                table: "enterprises",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}

