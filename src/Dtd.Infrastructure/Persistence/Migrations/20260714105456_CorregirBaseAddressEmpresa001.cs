using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dtd.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregirBaseAddressEmpresa001 : Migration
    {
        /// <summary>
        /// Corrige el <c>base_address</c> de la empresa 001 en la BD existente: el valor antiguo
        /// incluía <c>/api</c> (p.ej. <c>https://soluciona-iseries.gruposoledad.com/api</c>), pero
        /// el gateway ahora construye la URL absoluta como <c>{base_address}/api/enterprises/...</c>
        /// (el <c>/api</c> va en el path, no en la base). Dejar el valor antiguo produce un doble
        /// <c>/api</c> en la URL saliente. Se lleva a <c>base_address</c> = raíz del host (sin
        /// <c>/api</c>), igual que el seed (<c>docs/seed-empresa-001.sql</c>). Idempotente.
        /// Es una migración de DATOS (no hay cambios de modelo): el snapshot no se altera.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raíz del host SIN /api (el /api va en la ruta del gateway). Coincide con el seed.
            migrationBuilder.Sql(
                "UPDATE empresas SET base_address = 'https://soluciona-iseries.gruposoledad.com' " +
                "WHERE empresa = '001';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se revierte: el valor previo (con /api) era el bug. Re-aplicar Up es idempotente.
        }
    }
}

