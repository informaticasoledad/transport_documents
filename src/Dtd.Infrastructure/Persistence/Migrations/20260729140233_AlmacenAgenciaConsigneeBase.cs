using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlmacenAgenciaConsigneeBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "consignee_base_id",
                table: "almacen_agencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_almacen_agencias_consignee_base_id",
                table: "almacen_agencias",
                column: "consignee_base_id");

            migrationBuilder.AddForeignKey(
                name: "fk_almacen_agencias_consignees_consignee_base_id",
                table: "almacen_agencias",
                column: "consignee_base_id",
                principalTable: "consignees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_almacen_agencias_consignees_consignee_base_id",
                table: "almacen_agencias");

            migrationBuilder.DropIndex(
                name: "ix_almacen_agencias_consignee_base_id",
                table: "almacen_agencias");

            migrationBuilder.DropColumn(
                name: "consignee_base_id",
                table: "almacen_agencias");
        }
    }
}
