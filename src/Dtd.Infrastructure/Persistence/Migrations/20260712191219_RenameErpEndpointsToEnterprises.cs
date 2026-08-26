using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameErpEndpointsToEnterprises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_endpoints");

            migrationBuilder.CreateTable(
                name: "enterprises",
                columns: table => new
                {
                    empresa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    base_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    token_endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    timeout_seconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enterprises", x => x.empresa);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enterprises");

            migrationBuilder.CreateTable(
                name: "erp_endpoints",
                columns: table => new
                {
                    empresa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    api_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    base_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    timeout_seconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_erp_endpoints", x => x.empresa);
                });
        }
    }
}

