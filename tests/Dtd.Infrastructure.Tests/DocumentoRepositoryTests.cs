using Microsoft.Extensions.Logging;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Infrastructure.Persistence;
using Dtd.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dtd.Infrastructure.Tests;

/// <summary>Reproducción del flujo real (generar → asignar conductor) contra un proveedor relacional
/// (SQLite) que sí aplica FKs y lanza DbUpdateConcurrencyException en UPDATEs que no matchean filas.
/// El InMemory no reproduce ni lo uno ni lo otro.</summary>
public class DocumentoRepositoryTests
{
    private static DtdDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<DtdDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;
        var ctx = new DtdDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static (SqliteConnection Connection, IDisposable Dispose) CreateConnection()
    {
        // :memory: con shared cache: la BD vive mientras la conexión esté abierta. Necesario para que
        // EnsureCreated de un contexto la vea el siguiente contexto (cada request usa su propio ctx).
        var connection = new SqliteConnection("DataSource=file:memdb-repro?mode=memory&cache=shared");
        connection.Open();
        return (connection, connection);
    }

    private static Almacen CrearAlmacen() => Almacen.Crear(
        "001", "21", "GETAFE",
        calle: "Bell 2", codigoPostal: "28906", municipio: "Getafe", pais: "ES",
        email: "a@b.com", telefono: "911000000");

    private static Agencia CrearAgencia() => Agencia.Crear("001", "AG01", "Agencia 01");

    private static DocumentoDigitalTransporte CrearDocumento(Guid almacenId, Guid agenciaId) =>
        DocumentoDigitalTransporte.Crear(
            "001", almacenId, agenciaId,
            OrigenDocumento.Create("21", "DEL", "CALLE", null, "09200", "CIUDAD", "PROV", "ESPAÑA", "ES"),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);

    private static Conductor CrearConductorCatalogo() => Conductor.Crear(
        "001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null,
        taxId: "12345678Z", licensePlate: "1234ABC");

    [Fact]
    public async Task AsignarConductor_tras_cargar_sin_Include_Conductores_persiste_sin_concurrency()
    {
        // 1) Genera el documento (INSERT) en un contexto y ciérralo.
        var (conn, dispose) = CreateConnection();
        Guid documentoId = Guid.Empty;
        try
        {
            await using (var ctx = CreateContext(conn))
            {
                var almacen = CrearAlmacen();
                var agencia = CrearAgencia();
                ctx.Almacenes.Add(almacen);
                ctx.Agencias.Add(agencia);
                await ctx.SaveChangesAsync();

                var documento = CrearDocumento(almacen.Id, agencia.Id);
                var destino = DestinoExpedicion.Create("ES", "08", "08001", "Barcelona", "10");
                documento.AddExpedicion(Expedicion.CrearDesdeErp(
                    "EXP-1", "DOC-1", "C-1", 1, "001", almacen.Id, agencia.Id,
                    new DateOnly(2026, 7, 1), "1001", destino, 2));
                ctx.Documentos.Add(documento);
                await ctx.SaveChangesAsync();
                documentoId = documento.Id;
            }

            // 2) Carga por id en un contexto NUEVO (como hace el handler) y asigna un conductor.
            await using (var ctx = CreateContext(conn))
            {
                var repo = new DocumentoRepository(ctx);
                var documento = await repo.GetByIdAsync(documentoId, CancellationToken.None);
                documento.Should().NotBeNull();

                documento!.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(CrearConductorCatalogo()));

                // 3) Save: si GetByIdAsync no Include Conductores, EF trata el nuevo ConductorAsignado
                //    como existente (UPDATE) y no matchea filas → DbUpdateConcurrencyException.
                var act = async () => await ctx.SaveChangesAsync();

                // Si esto falla con DbUpdateConcurrencyException, hemos reproducido el bug.
                await act.Should().NotThrowAsync();
                documento.Conductores.Should().ContainSingle();
            }
        }
        finally
        {
            dispose.Dispose();
        }
    }
}