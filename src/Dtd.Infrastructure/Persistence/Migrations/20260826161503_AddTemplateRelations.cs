using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencias_templates",
                table: "almacen_agencias");

            migrationBuilder.AlterColumn<Guid>(
                name: "template_id",
                table: "almacen_agencias",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencias_templates",
                table: "almacen_agencias",
                column: "template_id",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_templates_empresas",
                table: "templates",
                column: "empresa",
                principalTable: "empresas",
                principalColumn: "empresa",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencias_templates",
                table: "almacen_agencias");

            migrationBuilder.DropForeignKey(
                name: "fk_templates_empresas",
                table: "templates");

            migrationBuilder.AlterColumn<Guid>(
                name: "template_id",
                table: "almacen_agencias",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencias_templates",
                table: "almacen_agencias",
                column: "template_id",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
