using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoWorkflowYReintentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ultimo_error",
                table: "documentos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ultimo_error_en",
                table: "documentos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "intentos_envio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    momento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    estado_http = table.Column<int>(type: "integer", nullable: true),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intentos_envio", x => x.id);
                    table.ForeignKey(
                        name: "fk_intentos_envio_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_intentos_envio_documento_id",
                table: "intentos_envio",
                column: "documento_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "intentos_envio");

            migrationBuilder.DropColumn(
                name: "ultimo_error",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "ultimo_error_en",
                table: "documentos");
        }
    }
}

