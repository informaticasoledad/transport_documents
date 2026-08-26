using Dtd.Application.Documentos.ConsigneesDocumento;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Consignees;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class AsignarConsigneeDocumentoCommandHandlerTests
{
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConsigneeRepository _consigneeRepository = Substitute.For<IConsigneeRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly AsignarConsigneeDocumentoCommandHandler _handler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AsignarConsigneeDocumentoCommandHandlerTests()
    {
        // El almacén y la agencia del documento existen por defecto (los tests que los anulan sobreescriben
        // el stub). El handler los resuelve por Id (FK); como los stubs de consignee usan ReturnsForAnyArgs,
        // el Id concreto es indiferente.
        _almacenRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Transportes Pepe"));

        _handler = new AsignarConsigneeDocumentoCommandHandler(
            _documentoRepository, _almacenRepository, _agenciaRepository, _consigneeRepository, _unitOfWork, _usuarioContexto);
    }

    private static Consignee CrearConsignee(string codigo = "CS01") =>
        Consignee.Crear("001", codigo, "Consignee " + codigo, Canal.Create("email")!,
            Movil.Create("600111222"), Email.Create("dest@x.com"), taxId: "B87654321");

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_documento_no_existe()
    {
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs((DocumentoDigitalTransporte?)null);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Documento.NoEncontrado");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(CrearDocumento());

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_almacen_del_documento_no_existe()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _almacenRepository.GetByIdAsync(default, default).ReturnsForAnyArgs((Almacen?)null);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.NoConfigurado");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_del_documento_no_existe()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _agenciaRepository.GetByIdAsync(default, default).ReturnsForAnyArgs((Agencia?)null);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Agencia.NoEncontrada");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_consignee_no_esta_vinculado_a_ambos()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs((Consignee?)null);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Consignee.NoEncontrado");
    }

    [Fact]
    public async Task Handle_devuelve_validation_si_el_consignee_esta_inactivo()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var inactivo = CrearConsignee();
        inactivo.Desactivar();
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs(inactivo);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, inactivo.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Code.Should().Be("Consignee.Inactivo");
    }

    [Fact]
    public async Task Handle_asigna_el_consignee_y_devuelve_su_dto()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var consignee = CrearConsignee("CS01");
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs(consignee);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, consignee.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Codigo.Should().Be("CS01");
        result.Value.Nombre.Should().Be("Consignee CS01");
        result.Value.Channel.Should().Be("email");
        documento.Consignees.Should().ContainSingle();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_reemplaza_el_consignee_anterior_y_queda_uno()
    {
        var documento = CrearDocumento();
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(CrearConsignee("CS01")));
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var cs02 = CrearConsignee("CS02");
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs(cs02);

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, cs02.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        documento.Consignees.Should().ContainSingle().Which.ConsigneeCodigo.Should().Be("CS02");
    }

    [Fact]
    public async Task Handle_devuelve_conflict_si_el_documento_ya_no_esta_en_Nuevo()
    {
        var documento = CrearDocumentoConductorConsigneeYEnviado();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs(CrearConsignee("CS03"));

        var result = await _handler.Handle(
            new AsignarConsigneeDocumentoCommand(documento.Id, Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        result.Errors[0].Code.Should().Be("Documento.YaConfirmado");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static DocumentoDigitalTransporte CrearDocumento() =>
        DocumentoDigitalTransporte.Crear(
            "001", AlmacenId, AgenciaId,
            OrigenDocumento.Create("21", "DEL", "CALLE", null, "09200", "CIUDAD", "PROV", "ESPAÑA", "ES"),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);

    private static DocumentoDigitalTransporte CrearDocumentoConductorConsigneeYEnviado()
    {
        var documento = CrearDocumento();
        var destino = DestinoExpedicion.Create("ES", "08", "08001", "Barcelona", "10");
        documento.AddExpedicion(Expedicion.CrearDesdeErp(
            "EXP-1", "DOC-1", "C-1", 1, "001", AlmacenId, AgenciaId, new DateOnly(2026, 7, 1), "1001", destino, 2));
        // Construye los envíos (envio_directo=false → 1 envío base): requisito de ValidarListoParaEnviar
        // antes de RegistrarEnvioExitoso.
        documento.ConstruirEnvios();
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear("001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null)));
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(CrearConsignee("CS01")));
        documento.RegistrarEnvioExitoso("LOT-X", EstadoDocuten.Pending, DateTimeOffset.UtcNow);
        return documento;
    }
}