using Dtd.Application.Documentos.ConductoresDocumento;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class AsignarConductoresDocumentoCommandHandlerTests
{
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConductorRepository _conductorRepository = Substitute.For<IConductorRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly AsignarConductoresDocumentoCommandHandler _handler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AsignarConductoresDocumentoCommandHandlerTests()
    {
        // La agencia del documento siempre existe por defecto (los tests que la anulan sobreescriben
        // el stub). El handler la resuelve por Id (FK) y la usa para buscar el conductor; como el stub
        // del repositorio de conductores usa ReturnsForAnyArgs/Arg.Any, el Id concreto es indiferente.
        _agenciaRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Transportes Pepe"));

        _handler = new AsignarConductoresDocumentoCommandHandler(
            _documentoRepository, _agenciaRepository, _conductorRepository, _unitOfWork, _usuarioContexto);
    }

    private static Conductor CrearConductor(string codigo, string channel = "sms", string? movil = "699000000", string? email = null,
        string? taxId = "12345678Z", string? licensePlate = "1234ABC")
    {
        var canal = Canal.Create(channel)!;
        return Conductor.Crear(
            "001", codigo, codigo == "C01" ? "Pepe" : "Conductor " + codigo,
            canal,
            movil is null ? null : Movil.Create(movil),
            email is null ? null : Email.Create(email),
            taxId, licensePlate);
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_documento_no_existe()
    {
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs((DocumentoDigitalTransporte?)null);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(Guid.NewGuid(), [Guid.NewGuid()]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Documento.NoEncontrado");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso_a_la_empresa()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(CrearDocumento());

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(Guid.NewGuid(), [Guid.NewGuid()]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_del_documento_no_existe()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _agenciaRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs((Agencia?)null);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [Guid.NewGuid()]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Agencia.NoEncontrada");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_un_conductor_no_existe_y_no_asigna_ninguno()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var c01 = CrearConductor("C01");
        var idInexistente = Guid.NewGuid();
        // El primer conductor existe, el segundo no → all-or-nothing: no se asigna ninguno.
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(c01.Id), Arg.Any<CancellationToken>())
            .Returns(c01);
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(idInexistente), Arg.Any<CancellationToken>())
            .Returns((Conductor?)null);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [c01.Id, idInexistente]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Conductor.NoEncontrado");
        documento.Conductores.Should().BeEmpty("all-or-nothing: el inexistente aborta la asignación");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_validation_si_un_conductor_esta_inactivo_y_no_asigna_ninguno()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var c01 = CrearConductor("C01");
        var inactivo = CrearConductor("C02");
        inactivo.Desactivar();
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(c01.Id), Arg.Any<CancellationToken>())
            .Returns(c01);
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(inactivo.Id), Arg.Any<CancellationToken>())
            .Returns(inactivo);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [c01.Id, inactivo.Id]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Code.Should().Be("Conductor.Inactivo");
        documento.Conductores.Should().BeEmpty("all-or-nothing: el inactivo aborta la asignación");
    }

    [Fact]
    public async Task Handle_asigna_varios_conductores_y_devuelve_sus_dtos()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var c01 = CrearConductor("C01");
        var c02 = CrearConductor("C02", channel: "email", movil: null, email: "a@b.com");
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(c01.Id), Arg.Any<CancellationToken>())
            .Returns(c01);
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(c02.Id), Arg.Any<CancellationToken>())
            .Returns(c02);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [c01.Id, c02.Id]), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Select(c => c.Codigo).Should().BeEquivalentTo(["C01", "C02"]);
        documento.Conductores.Should().HaveCount(2);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_es_idempotente_con_ids_repetidos_y_ya_asignados()
    {
        var documento = CrearDocumento();
        var conductor = CrearConductor("C01");
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(conductor));
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _conductorRepository.GetByAgenciaYIdAsync(default, default, default)
            .ReturnsForAnyArgs(conductor);

        // Mismo Id repetido en la lista y ya asignado previamente → no duplica.
        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [conductor.Id, conductor.Id]), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        documento.Conductores.Should().ContainSingle("no duplica el conductor ya asignado");
    }

    [Fact]
    public async Task Handle_devuelve_conflict_si_el_documento_ya_no_esta_en_Nuevo()
    {
        var documento = CrearDocumentoConductorYEnviado();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        var c03 = CrearConductor("C03");
        _conductorRepository.GetByAgenciaYIdAsync(Arg.Any<Guid>(), Arg.Is(c03.Id), Arg.Any<CancellationToken>())
            .Returns(c03);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [c03.Id]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        result.Errors[0].Code.Should().Be("Documento.YaConfirmado");
    }

    [Fact]
    public async Task Handle_no_resuelve_ningun_conductor_si_la_lista_es_vacia_o_solo_guids_vacios()
    {
        var documento = CrearDocumento();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);

        var result = await _handler.Handle(
            new AsignarConductoresDocumentoCommand(documento.Id, [Guid.Empty]), CancellationToken.None);

        // Tras descartar Guid.Empty, la lista de Ids únicos queda vacía → no se asigna nada (el
        // validator cubre la lista vacía real; aquí se prueba la robustez del handler con Guids vacíos).
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        documento.Conductores.Should().BeEmpty();
        await _conductorRepository.DidNotReceive().GetByAgenciaYIdAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static DocumentoDigitalTransporte CrearDocumento() =>
        DocumentoDigitalTransporte.Crear(
            "001", AlmacenId, AgenciaId,
            OrigenDocumento.Create("21", "DEL", "CALLE", null, "09200", "CIUDAD", "PROV", "ESPAÑA", "ES"),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);

    private static DocumentoDigitalTransporte CrearDocumentoConductorYEnviado()
    {
        var documento = CrearDocumento();
        var destino = DestinoExpedicion.Create("ES", "08", "08001", "Barcelona", "10");
        documento.AddExpedicion(Expedicion.CrearDesdeErp(
            "EXP-1", "DOC-1", "C-1", 1, "001", AlmacenId, AgenciaId, new DateOnly(2026, 7, 1), "1001", destino, 2));
        // Construye los envíos (envio_directo=false → 1 envío base): requisito de ValidarListoParaEnviar.
        documento.ConstruirEnvios();
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear("001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null)));
        // El documento exige exactamente 1 consignee para estar listo para enviar (ValidarListoParaEnviar).
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(
            Consignee.Crear("001", "CS01", "Destinatario Test", Canal.Create("email")!,
                Movil.Create("600111222"), Email.Create("dest@example.com"), taxId: "B87654321")));
        documento.RegistrarEnvioExitoso("LOT-X", EstadoDocuten.Pending, DateTimeOffset.UtcNow);
        return documento;
    }
}