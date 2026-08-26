using Dtd.Application.Documentos.EnviarDocumentoADocuten;
using Dtd.Application.Documentos.GenerarDocumento;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

/// <summary>
/// Flujo end-to-end de aplicación: generar (ingerir expediciones del ERP y agruparlas en envíos) →
/// asignar conductor + consignee → enviar a Docuten. Orquesta <see cref="GenerarDocumentoCommandHandler"/>
/// y <see cref="EnviarDocumentoADocutenCommandHandler"/> con gateways mockeados (ERP + Docuten), sin tocar
/// la BD. El paso de asignación se hace directo sobre el agregado (sus handlers están cubiertos en sus
/// propios tests) para mantener el foco en generar + agrupar + enviar.
/// </summary>
public class GenerarYEnviarDocumentoFlowTests
{
    // Mocks compartidos por ambos handlers.
    private readonly IExpedicionErpGateway _erpGateway = Substitute.For<IExpedicionErpGateway>();
    private readonly IDocumentoRepository _documentoRepository = Substitute.For<IDocumentoRepository>();
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    // Sólo para el handler de envío.
    private readonly IDocutenGateway _docutenGateway = Substitute.For<IDocutenGateway>();
    private readonly IEmpresaResolver _empresaResolver = Substitute.For<IEmpresaResolver>();
    private readonly DocutenMappingOptions _docutenMappingOptions = new() { DefaultLanguage = "es" };
    private readonly IDocutenDocumentoProvider _docutenDocumentoProvider = Substitute.For<IDocutenDocumentoProvider>();

    private readonly GenerarDocumentoCommandHandler _generarHandler;
    private readonly EnviarDocumentoADocutenCommandHandler _enviarHandler;

    private static readonly Guid AlmacenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AgenciaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly Almacen _almacen = Almacen.Crear(
        "001", "21", "DELEGACION MIRANDA",
        calle: "RIBERAS DEL EBRO N41 P.I.", codigoPostal: "09200", municipio: "MIRANDA DE EBRO", pais: "ES",
        email: "delegacionmiranda@gruposoledad.net", telefono: "947000000");

    private readonly Agencia _agencia = Agencia.Crear("001", "AG01", "Agencia 01"); // envio_directo=false por defecto

    public GenerarYEnviarDocumentoFlowTests()
    {
        // Master local: almacén y agencia válidos y disponibles para la tupla. El Id lo fija el agregado
        // (Guid.NewGuid); el handler lo usa para el documento (FK) y para EsAgenciaDisponibleAsync.
        _almacenRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(_almacen);
        _agenciaRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(_agencia);
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default).ReturnsForAnyArgs(true);

        // Ninguna expedición ya incluida → todas las del ERP son nuevas.
        _documentoRepository
            .ObtenerErpIdsIncluidosAsync(default!, default, default, default!, default)
            .ReturnsForAnyArgs(new HashSet<string>() as IReadOnlySet<string>);

        // Configuración del envío a Docuten (empresa + provider del PDF del shipment).
        _empresaResolver.ResolveAsync(default!, default)
            .ReturnsForAnyArgs(new EmpresaConfig("001", "http://erp", TaxId: "B12345678", Nombre: "Grupo Soledad"));
        _docutenDocumentoProvider.ObtenerDocumentoAsync(default!, default!, default)
            .ReturnsForAnyArgs(new DocutenDocumentoDto
            {
                DocumentType = "ecmr",
                DocumentName = "eCMR-EXP.pdf",
                ExternalId = "EXT-ECMR",
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

        _generarHandler = new GenerarDocumentoCommandHandler(
            _erpGateway, _documentoRepository, _almacenRepository, _agenciaRepository, _unitOfWork, _usuarioContexto);

        _enviarHandler = new EnviarDocumentoADocutenCommandHandler(
            _documentoRepository, _docutenGateway,
            _empresaResolver, _almacenRepository, _agenciaRepository, _docutenMappingOptions, _docutenDocumentoProvider,
            _unitOfWork, _usuarioContexto);
    }

    [Fact]
    public async Task Generar_agrupa_las_expediciones_en_un_solo_envio_y_se_envia_a_docuten()
    {
        // ── arrange: el ERP devuelve 3 expediciones (2 de cliente + 1 trasiego), bultos 2+3+1=6.
        // envio_directo=false → todas colapsan en 1 envío único a la base (mezcla permitida).
        _erpGateway.GetExpedicionesAsync(default!, default!, default!, default!, default)
            .ReturnsForAnyArgs(new List<ExpedicionErpDto>
            {
                CrearExpedicionErp("EXP-1", count: 0, isTransfer: false, detailCount: 2),
                CrearExpedicionErp("EXP-2", count: 1, isTransfer: false, detailCount: 3),
                CrearExpedicionErp("EXP-3", count: 2, isTransfer: true,  detailCount: 1),
            });

        // Capturamos el documento persistido por Generar (AddAsync) para inspeccionar la agrupación y
        // alimentar GetByIdAsync del handler de envío.
        DocumentoDigitalTransporte? documento = null;
        _documentoRepository.AddAsync(Arg.Do<DocumentoDigitalTransporte>(d => documento = d), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _documentoRepository.GetByIdAsync(default, default).ReturnsForAnyArgs(_ => documento!);

        // ── act 1: generar el documento (ingerir expediciones del ERP + agrupar en envíos).
        var generarResult = await _generarHandler.Handle(
            new GenerarDocumentoCommand("001", AlmacenId, AgenciaId, new DateOnly(2026, 7, 12), new DateOnly(2026, 7, 12)),
            CancellationToken.None);

        // ── assert 1: el documento nace con 3 expediciones agrupadas en 1 envío a la base.
        generarResult.IsError.Should().BeFalse();
        documento.Should().NotBeNull();
        documento!.Expediciones.Should().HaveCount(3);
        documento.Envios.Should().ContainSingle("envio_directo=false colapsa todas las expediciones en 1 envío");
        var envio = documento.Envios.Single();
        envio.EsDirecto.Should().BeFalse();
        envio.Destino.Should().BeNull();
        envio.Bultos.Should().Be(6, "suma los bultos de las 3 expediciones (2+3+1)");
        envio.ShipmentReference.Should().Be($"{documento.Id}#1");
        documento.Expediciones.All(e => e.EnvioId == envio.Id).Should().BeTrue("todas vinculadas al único envío");

        // ── arrange 2: el front asigna conductor + consignee (requisito para enviar con envio_directo=false).
        // Los handlers de asignación están cubiertos aparte; aquí los aplicamos directo al agregado.
        documento.AsignarConductor(ConductorAsignado.CrearDesdeCatalogo(
            Conductor.Crear("001", "C01", "Pepe",
                Canal.Create("sms")!, Movil.Create("699000000"), email: null,
                taxId: "12345678Z", licensePlate: "1234ABC")));
        documento.AsignarConsignee(ConsigneeAsignado.CrearDesdeCatalogo(
            Consignee.Crear("001", "CS01", "Destinatario Test",
                Canal.Create("email")!, Movil.Create("600111222"), Email.Create("dest@example.com"),
                taxId: "B87654321")));

        _docutenGateway.EnviarAsync(default!, default)
            .ReturnsForAnyArgs(new DocutenLoteEnvioResult("LOT-Y", EstadoDocuten.Pending));

        // ── act 2: enviar a Docuten (confirmar).
        var enviarResult = await _enviarHandler.Handle(
            new EnviarDocumentoADocutenCommand(documento.Id), CancellationToken.None);

        // ── assert 2: lote transmitido con 1 shipment que corresponde al envío agrupado; documento en Enviando.
        enviarResult.IsError.Should().BeFalse();
        enviarResult.Value.LotId.Should().Be("LOT-Y");
        documento.Estado.Should().Be(EstadoDocumento.Enviando);
        documento.DocutenId.Should().Be("LOT-Y");

        await _docutenGateway.Received(1).EnviarAsync(
            Arg.Is<DocutenLoteDto>(l =>
                l.LotReference == documento.Id.ToString()
                && l.Shipments.Count == 1
                && l.Shipments[0].ShipmentReference == $"{documento.Id}#1"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>Construye una <see cref="ExpedicionErpDto"/> con la estructura anidada real del ERP
    /// (origen común, destino por expedición, líneas de detalle). Los bultos del dominio derivan de
    /// <paramref name="detailCount"/> (nº de líneas). Refleja el shape de <c>ErpMockGateway</c>.</summary>
    private static ExpedicionErpDto CrearExpedicionErp(string erpId, int count, bool isTransfer, int detailCount) => new()
    {
        Id = erpId,
        Empresa = "001",
        DocumentNumber = $"2028140{100 + count:D3}",
        ExpeditionDate = new DateTime(2026, 7, 12),
        ExpeditionCode = $"11650{700 + count:D3}",
        ExpeditionType = isTransfer ? 2 : 1,
        OriginWarehouseId = "21",
        CustomerId = isTransfer ? null : (1000 + count).ToString(),
        DestinationWarehouseId = isTransfer ? "79" : null,
        ExpeditionOrigin = new ExpeditionOriginErpDto
        {
            Id = "21",
            AddressName = "DELEGACION MIRANDA",
            AddressStreet = "RIBERAS DEL EBRO N41 P.I.",
            AddressPhone1 = "",
            Zipcode = "09200",
            City = "MIRANDA DE EBRO",
            ProvinceName = "BURGOS",
            CountryName = "ESPAÑA",
            CountryIsoCode = "ES"
        },
        ExpeditionDestination = new ExpeditionDestinationErpDto
        {
            Id = isTransfer ? "79" : (1000 + count).ToString(),
            AddressName = isTransfer ? "VALLADOLID TALLER" : $"CLIENTE {1000 + count}",
            AddressStreet = $"C/ DESTINO {count}",
            AddressPhone1 = "920000000",
            Zipcode = (10000 + count).ToString(),
            City = isTransfer ? "Valladolid" : "Destino",
            ProvinceName = isTransfer ? "VALLADOLID" : "PROVINCIA",
            CountryName = "ESPAÑA",
            CountryIsoCode = "ES"
        },
        ExpeditionDetails = Enumerable.Range(0, detailCount)
            .Select(i => new ExpeditionDetailErpDto
            {
                ProductId = $"0101{i:D12}",
                ProductName = $"Neumático de prueba {count}-{i}",
                ProductUnits = 2m
            })
            .ToList()
    };
}