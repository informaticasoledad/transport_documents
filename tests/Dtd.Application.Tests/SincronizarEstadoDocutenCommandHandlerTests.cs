using Dtd.Application.Documentos.SincronizarEstadoDocuten;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class SincronizarEstadoDocutenCommandHandlerTests
{
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IDocutenGateway _docutenGateway = Substitute.For<IDocutenGateway>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly SincronizarEstadoDocutenCommandHandler _handler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public SincronizarEstadoDocutenCommandHandlerTests()
    {
        _handler = new SincronizarEstadoDocutenCommandHandler(_documentoRepository, _docutenGateway, _unitOfWork, _usuarioContexto);
    }

    [Fact]
    public async Task Handle_con_PendingDelivery_pasa_a_EnProgreso()
    {
        var documento = CrearDocumentoEnviado();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _docutenGateway.ObtenerEstadoAsync(default!, default)
            .ReturnsForAnyArgs(new DocutenLoteEstadoResult(
                EstadoDocuten.PendingDelivery,
                [new() { ShipmentStatus = EstadoDocuten.PendingDelivery }]));

        var result = await _handler.Handle(new SincronizarEstadoDocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        documento.Estado.Should().Be(EstadoDocumento.EnProgreso);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_con_Completed_pasa_a_Finalizado()
    {
        var documento = CrearDocumentoEnviado();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _docutenGateway.ObtenerEstadoAsync(default!, default)
            .ReturnsForAnyArgs(new DocutenLoteEstadoResult(
                EstadoDocuten.Completed,
                [new() { ShipmentStatus = EstadoDocuten.Completed }]));

        var result = await _handler.Handle(new SincronizarEstadoDocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        documento.Estado.Should().Be(EstadoDocumento.Finalizado);
    }

    [Fact]
    public async Task Handle_devuelve_error_si_no_ha_sido_enviado()
    {
        var documento = CrearDocumentoBase();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);

        var result = await _handler.Handle(new SincronizarEstadoDocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        await _docutenGateway.DidNotReceive().ObtenerEstadoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static OrigenDocumento CrearOrigen() =>
        OrigenDocumento.Create("21", "DELEGACION MIRANDA", "RIBERAS DEL EBRO", null, "09200",
            "MIRANDA DE EBRO", "BURGOS", "ESPAÑA", "ES");

    private static DocumentoDigitalTransporte CrearDocumentoBase() =>
        DocumentoDigitalTransporte.Crear(
            "001", AlmacenId, AgenciaId, CrearOrigen(),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);

    private static DocumentoDigitalTransporte CrearDocumentoEnviado()
    {
        var documento = CrearDocumentoBase();

        var destino = DestinoExpedicion.Create("ES", "08", "08001", "Barcelona", "10");
        documento.AddExpedicion(Expedicion.CrearDesdeErp(
            "EXP-1", "DOC-1", "C-1", expeditionType: 1,
            "001", AlmacenId, AgenciaId, new DateOnly(2026, 7, 1),
            cliente: "1001", destino, bultos: 2));
        // Construye los envíos (envio_directo=false → 1 envío base): requisito de ValidarListoParaEnviar
        // antes de RegistrarEnvioExitoso.
        documento.ConstruirEnvios();

        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear("001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null)));
        // El documento exige exactamente 1 consignee para estar listo para enviar (ValidarListoParaEnviar).
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(
            Consignee.Crear("001", "CS01", "Destinatario Test", Canal.Create("email")!,
                Movil.Create("600111222"), Email.Create("dest@example.com"), taxId: "B87654321")));
        documento.RegistrarEnvioExitoso("LOT-X", EstadoDocuten.Pending, DateTimeOffset.UtcNow);
        documento.ClearDomainEvents();
        return documento;
    }
}