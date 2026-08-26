using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocutenCallbackLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "docuten_callback_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recibido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lot_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lot_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    shipment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipment_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    @event = table.Column<string>(name: "event", type: "character varying(50)", maxLength: 50, nullable: true),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    procesado = table.Column<bool>(type: "boolean", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    headers = table.Column<string>(type: "text", nullable: true),
                    mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_docuten_callback_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_docuten_callback_logs_documento_id",
                table: "docuten_callback_logs",
                column: "documento_id");

            migrationBuilder.CreateIndex(
                name: "ix_docuten_callback_logs_lot_id",
                table: "docuten_callback_logs",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_docuten_callback_logs_recibido_en",
                table: "docuten_callback_logs",
                column: "recibido_en");

            migrationBuilder.CreateIndex(
                name: "ix_docuten_callback_logs_shipment_id",
                table: "docuten_callback_logs",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_docuten_callback_logs_shipment_reference",
                table: "docuten_callback_logs",
                column: "shipment_reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "docuten_callback_logs");
        }
    }
}
