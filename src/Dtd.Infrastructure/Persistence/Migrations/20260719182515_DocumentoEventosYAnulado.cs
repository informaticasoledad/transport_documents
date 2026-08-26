using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentoEventosYAnulado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Crear la nueva tabla de eventos antes de leer los intentos previos.
            migrationBuilder.CreateTable(
                name: "documento_eventos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    momento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estado_http = table.Column<int>(type: "integer", nullable: true),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documento_eventos", x => x.id);
                    table.ForeignKey(
                        name: "fk_documento_eventos_documentos_documento_id",
                        column: x => x.documento_id,
                        principalTable: "documentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documento_eventos_documento_id",
                table: "documento_eventos",
                column: "documento_id");

            // 2) Data-migration: reinterpretar los intentos_envio existentes como eventos del log.
            //    exitoso=true  ? EnvioADocuten (mensaje NULL; el tipo ya indica éxito)
            //    exitoso=false ? EnvioFallido  (conserva el mensaje de error y el estado HTTP)
            //    Los nombres 'EnvioADocuten'/'EnvioFallido' coinciden con el enum persistido como string.
            migrationBuilder.Sql(@"
INSERT INTO documento_eventos (id, documento_id, tipo, momento, estado_http, mensaje)
SELECT gen_random_uuid(), documento_id,
       CASE WHEN exitoso THEN 'EnvioADocuten' ELSE 'EnvioFallido' END,
       momento, estado_http,
       CASE WHEN exitoso THEN NULL ELSE mensaje END
FROM intentos_envio;
");

            // 3) Ya reinterpolarlos: drop de la tabla antigua y de las columnas de último error.
            migrationBuilder.DropTable(
                name: "intentos_envio");

            migrationBuilder.DropColumn(
                name: "ultimo_error",
                table: "documentos");

            migrationBuilder.DropColumn(
                name: "ultimo_error_en",
                table: "documentos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_eventos");

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
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_http = table.Column<int>(type: "integer", nullable: true),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false),
                    mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    momento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
    }
}

