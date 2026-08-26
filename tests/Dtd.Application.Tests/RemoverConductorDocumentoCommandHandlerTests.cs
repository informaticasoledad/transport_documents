using Dtd.Application.Documentos.ConductoresDocumento;
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

public class RemoverConductorDocumentoCommandHandlerTests
{
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly RemoverConductorDocumentoCommandHandler _handler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public RemoverConductorDocumentoCommandHandlerTests()
    {
        _handler = new RemoverConductorDocumentoCommandHandler(_documentoRepository, _unitOfWork, _usuarioContexto);
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_documento_no_existe()
    {
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs((DocumentoDigitalTransporte?)null);

        var result = await _handler.Handle(
            new RemoverConductorDocumentoCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Documento.NoEncontrado");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso_a_la_empresa()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(CrearDocumentoConConductores());

        var result = await _handler.Handle(
            new RemoverConductorDocumentoCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_conductor_no_esta_asignado()
    {
        var documento = CrearDocumentoConConductores();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);

        var result = await _handler.Handle(
            new RemoverConductorDocumentoCommand(documento.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Documento.ConductorNoAsignado");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_conflict_si_el_documento_ya_no_esta_en_Nuevo()
    {
        var documento = CrearDocumentoEnviado();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var conductorId = documento.Conductores.First().Id;

        var result = await _handler.Handle(
            new RemoverConductorDocumentoCommand(documento.Id, conductorId), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        result.Errors[0].Code.Should().Be("Documento.YaConfirmado");
    }

    [Fact]
    public async Task Handle_quita_el_conductor_y_persiste()
    {
        var documento = CrearDocumentoConConductores();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var conductorId = documento.Conductores.First(c => c.ConductorCodigo == "C01").Id;

        var result = await _handler.Handle(
            new RemoverConductorDocumentoCommand(documento.Id, conductorId), CancellationToken.None);

        result.IsError.Should().BeFalse();
        documento.Conductores.Should().HaveCount(1);
        documento.Conductores.Should().NotContain(c => c.ConductorCodigo == "C01");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static DocumentoDigitalTransporte CrearDocumento() =>
        DocumentoDigitalTransporte.Crear(
            "001", AlmacenId, AgenciaId,
            OrigenDocumento.Create("21", "DEL", "CALLE", null, "09200", "CIUDAD", "PROV", "ESPAÑA", "ES"),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);

    private static DocumentoDigitalTransporte CrearDocumentoConConductores()
    {
        var documento = CrearDocumento();
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear("001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null)));
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear("001", "C02", "Ana", Canal.Create("sms")!, Movil.Create("600000002"), email: null)));
        return documento;
    }

    private static DocumentoDigitalTransporte CrearDocumentoEnviado()
    {
        var documento = CrearDocumentoConConductores();
        var destino = DestinoExpedicion.Create("ES", "08", "08001", "Barcelona", "10");
        documento.AddExpedicion(Expedicion.CrearDesdeErp(
            "EXP-1", "DOC-1", "C-1", 1, "001", AlmacenId, AgenciaId, new DateOnly(2026, 7, 1), "1001", destino, 2));
        // Construye los envíos (envio_directo=false → 1 envío base): requisito de ValidarListoParaEnviar.
        documento.ConstruirEnvios();
        // El documento exige exactamente 1 consignee para estar listo para enviar (ValidarListoParaEnviar).
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(
            Consignee.Crear("001", "CS01", "Destinatario Test", Canal.Create("email")!,
                Movil.Create("600111222"), Email.Create("dest@example.com"), taxId: "B87654321")));
        documento.RegistrarEnvioExitoso("LOT-X", EstadoDocuten.Pending, DateTimeOffset.UtcNow);
        return documento;
    }
}