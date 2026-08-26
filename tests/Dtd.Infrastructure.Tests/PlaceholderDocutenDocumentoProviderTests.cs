using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Infrastructure.Gateways;
using FluentAssertions;

namespace Dtd.Infrastructure.Tests;

/// <summary>Tests del placeholder del PDF Docuten. Fija el comportamiento crítico validado por la API:
/// el PDF debe listar un <c>signer</c> por cada party firmante (consignor + N drivers + consignee) con su
/// mismo <c>order</c>, o Docuten rechaza con <c>"missing party order(s): [N]"</c>. Los CCs (copia, no
/// firman) NO generan signer.</summary>
public class PlaceholderDocutenDocumentoProviderTests
{
    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly PlaceholderDocutenDocumentoProvider _provider = new();

    private static DocumentoDigitalTransporte CrearDocumento(int numConductores, int numCcs)
    {
        var documento = DocumentoDigitalTransporte.Crear(
            "001", AlmacenId, AgenciaId,
            OrigenDocumento.Create("21", "DELEGACION MIRANDA", "RIBERAS DEL EBRO", null, "09200",
                "MIRANDA DE EBRO", "BURGOS", "ESPAÑA", "ES"),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);

        var destino = DestinoExpedicion.Create("ES", "08", "08001", "Barcelona", "10",
            addressName: "AUTOS STAR C.B.", addressStreet: "CL VIRGEN DE LA SOTERRAÑA 4 0");
        documento.AddExpedicion(Expedicion.CrearDesdeErp(
            "EXP-1", "DOC-1", "C-1", expeditionType: 1,
            "001", AlmacenId, AgenciaId, new DateOnly(2026, 7, 1),
            cliente: "1001", destino, bultos: 2));

        for (var i = 1; i <= numConductores; i++)
        {
            documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
                Conductor.Crear(
                    "001", $"C{i:00}", $"Conductor {i}",
                    Canal.Create("sms")!, Movil.Create($"69900000{i:00}"), email: null,
                    taxId: $"1234567{i:00}Z", licensePlate: $"123{i:00}ABC")));
        }

        // El consignee es requisito para enviar (ValidarListoParaEnviar); lo añadimos siempre.
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(
            Consignee.Crear(
                "001", "CS01", "Destinatario Test",
                Canal.Create("email")!, Movil.Create("600111222"), Email.Create("dest@example.com"),
                taxId: "B87654321")));

        for (var i = 1; i <= numCcs; i++)
        {
            documento.AsignarCc(CcAsignado.CrearDesdeCatalogo(
                Cc.Crear("001", $"CC{i}", $"CC {i}", Email.Create($"cc{i}@example.com")!)));
        }

        // Construye los envíos (shipments) a partir de las expediciones. Con envio_directo=false (default)
        // colapsa en 1 envío base con todas las expediciones; el provider ahora recibe el envío, no la
        // expedición.
        documento.ConstruirEnvios();

        return documento;
    }

    [Fact]
    public async Task Un_signer_por_party_firmante_consu_consignee_incluido()
    {
        // 1 driver → parties firmantes: consignor(1), driver(2), consignee(3). CCs no firman.
        var documento = CrearDocumento(numConductores: 1, numCcs: 2);
        var envio = documento.Envios.First();

        var dto = await _provider.ObtenerDocumentoAsync(documento, envio, CancellationToken.None);

        dto.Signable.Should().BeTrue();
        // 1 (consignor) + 1 (driver) + 1 (consignee) = 3 signers. Los 2 CCs NO generan signer.
        dto.Signers.Should().HaveCount(3);
        dto.Signers.Select(s => s.Order).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        dto.Signers.Should().AllSatisfy(s =>
        {
            s.Coordinate.Should().NotBeNull();
            s.Coordinate!.SigPage.Should().Be(0);
        });
    }

    [Fact]
    public async Task Con_N_conductores_el_consignee_tiene_order_2_mas_n()
    {
        // 2 drivers → consignor(1), drivers(2,3), consignee(4). CCs no firman.
        var documento = CrearDocumento(numConductores: 2, numCcs: 1);
        var envio = documento.Envios.First();

        var dto = await _provider.ObtenerDocumentoAsync(documento, envio, CancellationToken.None);

        // 1 (consignor) + 2 (drivers) + 1 (consignee) = 4 signers. El CC NO genera signer.
        dto.Signers.Should().HaveCount(4);
        dto.Signers.Select(s => s.Order).Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
    }
}