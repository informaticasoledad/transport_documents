using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class templates1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "template_id",
                table: "almacen_agencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencias_template_id",
                table: "almacen_agencias",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_templates_empresa_code",
                table: "templates",
                columns: new[] { "empresa", "code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencias_templates",
                table: "almacen_agencias",
                column: "template_id",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencias_templates",
                table: "almacen_agencias");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropIndex(
                name: "ix_almacen_agencias_template_id",
                table: "almacen_agencias");

            migrationBuilder.DropColumn(
                name: "template_id",
                table: "almacen_agencias");
        }
    }
}
