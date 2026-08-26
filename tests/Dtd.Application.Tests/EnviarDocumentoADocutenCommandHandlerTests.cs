using Dtd.Application.Documentos.EnviarDocumentoADocuten;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class EnviarDocumentoADocutenCommandHandlerTests
{
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IDocutenGateway _docutenGateway = Substitute.For<IDocutenGateway>();
    private readonly IEmpresaResolver _empresaResolver = Substitute.For<IEmpresaResolver>();
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly DocutenMappingOptions _docutenMappingOptions = new() { DefaultLanguage = "es" };
    private readonly IDocutenDocumentoProvider _docutenDocumentoProvider = Substitute.For<IDocutenDocumentoProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly EnviarDocumentoADocutenCommandHandler _handler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static Almacen AlmacenMock() => Almacen.Crear(
        "001", "21", "GETAFE ALMACEN",
        calle: "Bell 2", codigoPostal: "28906", municipio: "Getafe", pais: "ES",
        email: "delegacionmadrid@gruposoledad.net", telefono: "911910910");

    private static Agencia AgenciaMock() => Agencia.Crear("001", "AG01", "Agencia 01");

    public EnviarDocumentoADocutenCommandHandlerTests()
    {
        _empresaResolver.ResolveAsync(default!, default)
            .ReturnsForAnyArgs(new EmpresaConfig("001", "http://erp", TaxId: "B12345678", Nombre: "Grupo Soledad"));
        _almacenRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(AlmacenMock());
        _agenciaRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(AgenciaMock());
        // El provider del PDF del shipment (documents[]) se mockea; ignora las entradas (ReturnsForAnyArgs).
        // DocutenDocumentoDto es un record con props required init y sin constructor primario → inicializador.
        _docutenDocumentoProvider.ObtenerDocumentoAsync(default!, default!, default)
            .ReturnsForAnyArgs(new DocutenDocumentoDto
            {
                DocumentType = "ecmr",
                DocumentName = "eCMR-EXP-1.pdf",
                ExternalId = "EXT-ECMR-EXP-1",
                Content = "placeholder-base64",
                Signable = true,
                Signers =
                [
                    new DocutenSignerDto
                    {
                        Order = 1,
                        Coordinate = new DocutenSignerCoordinateDto
                        {
                            SigPage = 0, TopLeftCornerX = 120, TopLeftCornerY = 650, Width = 180, Height = 60
                        }
                    }
                ]
            });

        _handler = new EnviarDocumentoADocutenCommandHandler(
            _documentoRepository, _docutenGateway,
            _empresaResolver, _almacenRepository, _agenciaRepository, _docutenMappingOptions, _docutenDocumentoProvider,
            _unitOfWork, _usuarioContexto);
    }

    [Fact]
    public async Task Handle_envia_a_docuten_con_los_conductores_asignados()
    {
        var documento = CrearDocumentoConExpedicionYConductor();
        _documentoRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(documento);

        _docutenGateway.EnviarAsync(default!, default)
            .ReturnsForAnyArgs(new DocutenLoteEnvioResult("LOT-X", EstadoDocuten.Pending));

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.LotId.Should().Be("LOT-X");
        documento.Estado.Should().Be(EstadoDocumento.Enviando);
        documento.TieneConductores.Should().BeTrue();

        await _docutenGateway.Received(1).EnviarAsync(
            Arg.Is<DocutenLoteDto>(l => l.LotReference == documento.Id.ToString()
                && l.Shipments.Count == 1
                && l.Shipments[0].Parties.Drivers.Count == 1
                // Driver = snapshot del catálogo: perfil completo + canal explícito + order=2.
                && l.Shipments[0].Parties.Drivers[0].Name == "Pepe"
                && l.Shipments[0].Parties.Drivers[0].TaxId == "12345678Z"
                && l.Shipments[0].Parties.Drivers[0].LicensePlate == "1234ABC"
                && l.Shipments[0].Parties.Drivers[0].Channel == "sms"
                && l.Shipments[0].Parties.Drivers[0].Mobile == "+699000000"
                && l.Shipments[0].Parties.Drivers[0].Order == 2
                && l.Shipments[0].Parties.Drivers[0].SigningRole == "signer"
                && l.Shipments[0].Parties.Drivers[0].SignatureType == "biometric"
                // Consignee va detrás de los N drivers (1 driver → consignee order=3).
                && l.Shipments[0].Parties.Consignees[0].Order == 3
                // Consignee = firmante (signer/biometric/sms) con el móvil del último conductor (ya no "cc").
                && l.Shipments[0].Parties.Consignees[0].Name == "Destinatario Test"
                && l.Shipments[0].Parties.Consignees[0].TaxId == "B87654321"
                && l.Shipments[0].Parties.Consignees[0].SigningRole == "signer"
                && l.Shipments[0].Parties.Consignees[0].SignatureType == "biometric"
                && l.Shipments[0].Parties.Consignees[0].Channel == "sms"
                && l.Shipments[0].Parties.Consignees[0].Email == "dest@example.com"
                && l.Shipments[0].Parties.Consignees[0].Mobile == "+699000000"
                && l.Shipments[0].Parties.Consignees[0].Language == "es"
                // Sin CCs asignados → el array consignees lleva sólo el consignee firmante.
                && l.Shipments[0].Parties.Consignees.Count == 1
                && l.Shipments[0].Origin.CountryCode == "ES"
                // Consignor: name/tax_id/signer_* de la empresa; dirección + contacto del almacén.
                && l.Shipments[0].Parties.Consignors[0].TaxId == "B12345678"
                && l.Shipments[0].Parties.Consignors[0].Name == "Grupo Soledad"
                && l.Shipments[0].Parties.Consignors[0].SignerName == "Grupo Soledad"
                && l.Shipments[0].Parties.Consignors[0].SignerTaxId == "B12345678"
                && l.Shipments[0].Parties.Consignors[0].SignatureType == "automated"
                && l.Shipments[0].Parties.Consignors[0].Address == "Bell 2"
                && l.Shipments[0].Parties.Consignors[0].PostCode == "28906"
                && l.Shipments[0].Parties.Consignors[0].City == "Getafe"
                && l.Shipments[0].Parties.Consignors[0].CountryCode == "ES"
                && l.Shipments[0].Parties.Consignors[0].Email == "delegacionmadrid@gruposoledad.net"
                && l.Shipments[0].Parties.Consignors[0].Mobile == "+911910910"
                && l.Shipments[0].Parties.Consignors[0].Channel == "email"
                && l.Shipments[0].Parties.Consignors[0].RedirectUrl == null
                // goods: Docuten exige dangerous_goods NOT NULL → el mapper lo fija a false por defecto.
                && l.Shipments[0].Goods.Count == 1
                && l.Shipments[0].Goods[0].DangerousGoods == false
                // documents[] es obligatorio en Docuten: el provider aporta un eCMR por shipment.
                // (En árbol de expresión no se permite 'is not null' ni fluye el análisis de nulabilidad → '!=' + '!')
                && l.Shipments[0].Documents != null
                && l.Shipments[0].Documents!.Count == 1
                && l.Shipments[0].Documents![0].DocumentType == "ecmr"
                && l.Shipments[0].CallbackUrl == "" // callback vacío (no null) por decisión de envío
                && l.CallbackUrl == ""
                && l.Shipments[0].Metadata.Any(m => m.Name == "empresa" && m.Value == "001")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_envia_ccs_como_consignees_cc_email_tras_el_consignee_firmante()
    {
        // 1 driver (con móvil), 1 consignee firmante y 2 CCs en copia.
        var documento = CrearDocumentoConExpedicionConductorConsigneeYCcs();
        _documentoRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs(documento);

        _docutenGateway.EnviarAsync(default!, default)
            .ReturnsForAnyArgs(new DocutenLoteEnvioResult("LOT-CC", EstadoDocuten.Pending));

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.LotId.Should().Be("LOT-CC");

        await _docutenGateway.Received(1).EnviarAsync(
            Arg.Is<DocutenLoteDto>(l =>
                // consignees[0] = consignee firmante (signer/biometric/sms, móvil del último conductor).
                l.Shipments[0].Parties.Consignees.Count == 3
                && l.Shipments[0].Parties.Consignees[0].SigningRole == "signer"
                && l.Shipments[0].Parties.Consignees[0].SignatureType == "biometric"
                && l.Shipments[0].Parties.Consignees[0].Channel == "sms"
                && l.Shipments[0].Parties.Consignees[0].Mobile == "+699000000"
                && l.Shipments[0].Parties.Consignees[0].Order == 3
                // consignees[1..] = CCs en copia (cc/email), order detrás del consignee (1 driver → 4, 5).
                && l.Shipments[0].Parties.Consignees[1].SigningRole == "cc"
                && l.Shipments[0].Parties.Consignees[1].Channel == "email"
                && l.Shipments[0].Parties.Consignees[1].Email == "cc1@example.com"
                && l.Shipments[0].Parties.Consignees[1].Name == "CC Uno"
                && l.Shipments[0].Parties.Consignees[1].Order == 4
                && l.Shipments[0].Parties.Consignees[1].SignatureType == null
                && l.Shipments[0].Parties.Consignees[2].SigningRole == "cc"
                && l.Shipments[0].Parties.Consignees[2].Channel == "email"
                && l.Shipments[0].Parties.Consignees[2].Email == "cc2@example.com"
                && l.Shipments[0].Parties.Consignees[2].Name == "CC Dos"
                && l.Shipments[0].Parties.Consignees[2].Order == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_error_failure_si_el_almacen_no_existe()
    {
        var documento = CrearDocumentoConExpedicionYConductor();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _almacenRepository.GetByIdAsync(default, default)
            .ReturnsForAnyArgs((Almacen?)null);

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        result.Errors[0].Code.Should().Be("Almacen.NoConfigurado");
        documento.Estado.Should().Be(EstadoDocumento.Nuevo, "sin almacén no se construye el consignor ni se envía");
        await _docutenGateway.DidNotReceive().EnviarAsync(Arg.Any<DocutenLoteDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_validation_si_no_hay_conductores_asignados()
    {
        var documento = CrearDocumentoConExpedicion(); // sin conductor
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Code.Should().Be("Documento.ConductorRequerido");
        documento.Estado.Should().Be(EstadoDocumento.Nuevo);
        documento.TieneConductores.Should().BeFalse();
        await _docutenGateway.DidNotReceive().EnviarAsync(Arg.Any<DocutenLoteDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_validation_si_no_hay_envios()
    {
        // Un documento Nuevo con conductor pero sin expediciones (→ sin envíos): hoy es inalcanzable vía
        // la API (generar garantiza ≥1 expedición y construye los envíos, y no hay endpoint para quitarlas),
        // pero la regla se valida antes de transmitir para no enviar un lote vacío a Docuten
        // (defense-in-depth). ConstruirEnvios no produce envíos si no hay expediciones.
        var documento = CrearDocumentoConConductorSinExpedicion();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].Code.Should().Be("Documento.EnvioRequerido");
        documento.Estado.Should().Be(EstadoDocumento.Nuevo);
        documento.Expediciones.Should().BeEmpty();
        documento.Envios.Should().BeEmpty();
        await _docutenGateway.DidNotReceive().EnviarAsync(Arg.Any<DocutenLoteDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_error_failure_si_la_empresa_no_esta_configurada()
    {
        var documento = CrearDocumentoConExpedicionYConductor();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _empresaResolver.ResolveAsync(default!, default).ReturnsForAnyArgs((EmpresaConfig?)null);

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        await _docutenGateway.DidNotReceive().EnviarAsync(Arg.Any<DocutenLoteDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_registra_intento_fallido_y_devuelve_error_si_el_gateway_falla()
    {
        var documento = CrearDocumentoConExpedicionYConductor();
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);
        _docutenGateway.EnviarAsync(default!, default)
            .ReturnsForAnyArgs<DocutenLoteEnvioResult>(_ => throw new HttpRequestException("500"));

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        documento.Estado.Should().Be(EstadoDocumento.Nuevo, "el fallo no avanza el pipeline");
        documento.Eventos.Should().ContainSingle(e => e.Tipo == DocumentoEventoTipo.EnvioFallido)
            .Which.Mensaje.Should().NotBeNullOrEmpty();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _docutenGateway.Received(1).EnviarAsync(Arg.Any<DocutenLoteDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_conflicto_si_el_documento_ya_esta_confirmado()
    {
        var documento = CrearDocumentoConExpedicionYConductor();
        documento.RegistrarEnvioExitoso("LOT-X", EstadoDocuten.Pending, DateTimeOffset.UtcNow);
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(documento);

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
        await _docutenGateway.DidNotReceive().EnviarAsync(Arg.Any<DocutenLoteDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_documento_no_existe()
    {
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs((DocumentoDigitalTransporte?)null);

        var result = await _handler.Handle(new EnviarDocumentoADocutenCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    private static DocumentoDigitalTransporte CrearDocumentoConExpedicion()
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
        // Construye los envíos (envio_directo=false por defecto → 1 envío base con la expedición). Sin
        // esto, ValidarListoParaEnviar falla con EnvioRequerido antes que cualquier otra regla.
        documento.ConstruirEnvios();
        return documento;
    }

    private static DocumentoDigitalTransporte CrearDocumentoConExpedicionYConductor()
    {
        var documento = CrearDocumentoConExpedicion();
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear(
                "001", "C01", "Pepe",
                Canal.Create("sms")!, Movil.Create("699000000"), email: null,
                taxId: "12345678Z", licensePlate: "1234ABC")));
        // El documento exige exactamente 1 consignee para estar listo para enviar (ValidarListoParaEnviar).
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(CrearConsignee()));
        return documento;
    }

    private static Consignee CrearConsignee() =>
        Consignee.Crear(
            "001", "CS01", "Destinatario Test",
            Canal.Create("email")!, Movil.Create("600111222"), Email.Create("dest@example.com"),
            taxId: "B87654321");

    private static Cc CrearCc(string codigo, string nombre, string email) =>
        Cc.Crear("001", codigo, nombre, Email.Create(email)!);

    private static DocumentoDigitalTransporte CrearDocumentoConExpedicionConductorConsigneeYCcs()
    {
        var documento = CrearDocumentoConExpedicionYConductor();
        documento.AsignarCc(CcAsignado.CrearDesdeCatalogo(CrearCc("CC1", "CC Uno", "cc1@example.com")));
        documento.AsignarCc(CcAsignado.CrearDesdeCatalogo(CrearCc("CC2", "CC Dos", "cc2@example.com")));
        return documento;
    }

    private static DocumentoDigitalTransporte CrearDocumentoConConductorSinExpedicion()
    {
        var documento = DocumentoDigitalTransporte.Crear(
            "001", AlmacenId, AgenciaId,
            OrigenDocumento.Create("21", "DELEGACION MIRANDA", "RIBERAS DEL EBRO", null, "09200",
                "MIRANDA DE EBRO", "BURGOS", "ESPAÑA", "ES"),
            RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5)),
            usuario: null, DateTimeOffset.UtcNow);
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear(
                "001", "C01", "Pepe",
                Canal.Create("sms")!, Movil.Create("699000000"), email: null,
                taxId: "12345678Z", licensePlate: "1234ABC")));
        return documento;
    }
}