using Dtd.Application.Documentos.GenerarDocumento;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class GenerarDocumentoCommandHandlerTests
{
    private readonly IExpedicionErpGateway _erpGateway = Substitute.For<IExpedicionErpGateway>();
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly GenerarDocumentoCommandHandler _handler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public GenerarDocumentoCommandHandlerTests()
    {
        // Por defecto el almacén y la agencia existen para la empresa y la agencia está disponible →
        // la validación de master local pasa y se alcanza el ERP (los tests que validan contra la
        // master sobreescriben estos stubs).
        _almacenRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Agencia 01"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(true);

        // El back no auto-adjunta conductores al generar: el documento nace sin conductores y el
        // front los añade antes de confirmar (confirmar valida ≥1 + canal).
        // Current == null por defecto en NSubstitute → Auth deshabilitada (dev): sin chequeo de empresa.
        _handler = new GenerarDocumentoCommandHandler(
            _erpGateway, _documentoRepository, _almacenRepository, _agenciaRepository, _unitOfWork, _usuarioContexto);
    }

    [Fact]
    public async Task Handle_genera_documento_solo_con_expediciones_no_incluidas()
    {
        var expediciones = new List<ExpedicionErpDto>
        {
            CrearExpedicion("EXP-1"),
            CrearExpedicion("EXP-2"),
            CrearExpedicion("EXP-3")
        };
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(expediciones);
        _documentoRepository.ObtenerErpIdsIncluidosAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(new HashSet<string> { "EXP-2" });

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();

        await _documentoRepository.Received(1).AddAsync(
            Arg.Is<DocumentoDigitalTransporte>(d => d.Expediciones.Count == 2
                && d.Expediciones.All(e => e.ErpId != "EXP-2")
                && d.Origen.WarehouseId == "21"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_error_si_el_erp_no_tiene_expediciones()
    {
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(new List<ExpedicionErpDto>());

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_devuelve_conflicto_si_todas_ya_estan_incluidas()
    {
        var expediciones = new List<ExpedicionErpDto> { CrearExpedicion("EXP-1") };
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(expediciones);
        _documentoRepository.ObtenerErpIdsIncluidosAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(new HashSet<string> { "EXP-1" });

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        await _documentoRepository.DidNotReceive().AddAsync(Arg.Any<DocumentoDigitalTransporte>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_failure_si_la_empresa_no_tiene_endpoint_erp_configurado()
    {
        // El gateway real lanza EmpresaNoConfiguradaException cuando no hay fila en empresas.
        // La empresa "998" no tiene endpoint ERP: el master local sí existe (para no fallar antes
        // en la validación de almacén/agencia), pero el ERP lanza al no encontrar la fila.
        _almacenRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Almacen.Crear("998", "21", "GETAFE"));
        _agenciaRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(Agencia.Crear("998", "AG01", "Agencia 01"));
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs<IReadOnlyList<ExpedicionErpDto>>(_ => throw new EmpresaNoConfiguradaException("998"));

        var command = new GenerarDocumentoCommand("998", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        result.Errors[0].Code.Should().Be("Empresa.ErpNoConfigurado");
        await _documentoRepository.DidNotReceive().AddAsync(Arg.Any<DocumentoDigitalTransporte>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_recorta_la_empresa_y_deja_pasar_si_el_usuario_tiene_acceso()
    {
        _usuarioContexto.Current
            .Returns(new UsuarioInfo("user-1", "Fran", new HashSet<string> { "1" }));
        var expediciones = new List<ExpedicionErpDto> { CrearExpedicion("EXP-1") };
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(expediciones);
        _documentoRepository.ObtenerErpIdsIncluidosAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(new HashSet<string>());

        // "1" se normaliza a "001", que está en las empresas del usuario → pasa.
        var command = new GenerarDocumentoCommand(" 1 ", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        await _documentoRepository.Received(1).AddAsync(
            Arg.Is<DocumentoDigitalTransporte>(d => d.Empresa == "1" && d.Usuario == "user-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso_a_la_empresa()
    {
        _usuarioContexto.Current
            .Returns(new UsuarioInfo("user-1", "Fran", new HashSet<string> { "002" }));

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.Errors[0].Code.Should().Be("Empresa.NoAutorizada");
        // No se llega a llamar al ERP cuando el usuario no tiene acceso.
        await _erpGateway.DidNotReceive().GetExpedicionesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dtd.Domain.Documentos.ValueObjects.RangoFechas>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_validation_si_almacen_no_existe()
    {
        _almacenRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs((Almacen?)null);

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Code.Should().Be("Almacen.NoConfigurado");
        // No se llama al ERP cuando falla la validación de master local.
        await _erpGateway.DidNotReceive().GetExpedicionesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dtd.Domain.Documentos.ValueObjects.RangoFechas>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_validation_si_agencia_no_disponible_para_almacen()
    {
        // El almacén existe (stub por defecto), pero la agencia no está en la unión almacen_agencias.
        _almacenRepository.EsAgenciaDisponibleAsync(default!, default!, default)
            .ReturnsForAnyArgs(false);

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Code.Should().Be("Almacen.AgenciaNoDisponible");
        await _erpGateway.DidNotReceive().GetExpedicionesAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dtd.Domain.Documentos.ValueObjects.RangoFechas>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_genera_documento_sin_conductores_el_back_no_auto_adjunta()
    {
        var expediciones = new List<ExpedicionErpDto> { CrearExpedicion("EXP-1") };
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(expediciones);
        _documentoRepository.ObtenerErpIdsIncluidosAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(new HashSet<string>());

        var command = new GenerarDocumentoCommand("001", AlmacenId, AgenciaId,
            new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        // El documento nace sin conductores: el back no auto-adjunta los defaults de la tupla;
        // los añade el front antes de confirmar.
        await _documentoRepository.Received(1).AddAsync(
            Arg.Is<DocumentoDigitalTransporte>(d => d.Conductores.Count == 0),
            Arg.Any<CancellationToken>());
    }

    private static ExpedicionErpDto CrearExpedicion(string erpId) => new()
    {
        Id = erpId,
        // Empresa no viaja en el body del ERP; el gateway real la estampa con `with`. El mock de
        // NSubstitute devuelve la lista tal cual, así que la fijamos aquí para el test. El almacén y
        // la agencia se persisten por Id (FK) desde el documento; ya no hay AgenciaCodigo en el DTO.
        Empresa = "001",
        DocumentNumber = "DOC-" + erpId,
        ExpeditionDate = new DateTime(2026, 7, 1),
        ExpeditionCode = "C-" + erpId,
        ExpeditionType = 1,
        OriginWarehouseId = "21",
        CustomerId = "1001",
        DestinationWarehouseId = null,
        ExpeditionOrigin = new ExpeditionOriginErpDto
        {
            Id = "21",
            AddressName = "DELEGACION MIRANDA",
            AddressStreet = "RIBERAS DEL EBRO",
            Zipcode = "09200",
            City = "MIRANDA DE EBRO",
            ProvinceName = "BURGOS",
            CountryName = "ESPAÑA",
            CountryIsoCode = "ES"
        },
        ExpeditionDestination = new ExpeditionDestinationErpDto
        {
            Id = "10",
            AddressName = "Cliente",
            Zipcode = "08001",
            City = "Barcelona",
            ProvinceName = "08",
            CountryName = "ESPAÑA",
            CountryIsoCode = "ES"
        },
        ExpeditionDetails = new List<ExpeditionDetailErpDto>
        {
            new() { ProductId = "P1", ProductName = "Neu", ProductUnits = 2m }
        }
    };
}
