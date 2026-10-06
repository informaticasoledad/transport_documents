using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class resendnotif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_ultimo_reenvio_notificacion",
                table: "documento_envios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numero_reenvios_notificacion",
                table: "documento_envios",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_ultimo_reenvio_notificacion",
                table: "documento_envios");

            migrationBuilder.DropColumn(
                name: "numero_reenvios_notificacion",
                table: "documento_envios");
        }
    }
}
