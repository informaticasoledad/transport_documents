namespace Dtd.Application.Documentos;

public sealed record DocumentoDto
{
    public Guid Id { get; init; }
    public string Empresa { get; init; } = string.Empty;
    public Guid AlmacenId { get; init; }
    public string AlmacenCodigo { get; init; } = string.Empty;
    public string AlmacenNombre { get; init; } = string.Empty;
    public Guid AgenciaId { get; init; }
    public string AgenciaCodigo { get; init; } = string.Empty;
    public string AgenciaNombre { get; init; } = string.Empty;
    public OrigenDto Origen { get; init; } = new();
    public DateOnly FechaDesde { get; init; }
    public DateOnly FechaHasta { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string? PlataformaId { get; init; }
    public string? PlataformaEstado { get; init; }
    public IReadOnlyList<ConductorDto> Conductores { get; init; } = [];
    public IReadOnlyList<CcDto> Ccs { get; init; } = [];
    public string? Usuario { get; init; }
    public DateTimeOffset FechaGeneracion { get; init; }
    public DateTimeOffset ActualizadoEn { get; init; }
    public int TotalExpediciones { get; init; }
    public IReadOnlyList<ExpedicionDto> Expediciones { get; init; } = [];
    public bool EnvioDirecto { get; init; }
    public IReadOnlyList<EnvioDto> Envios { get; init; } = [];
}

public sealed record EnvioDto
{
    public Guid Id { get; init; }
    public int Orden { get; init; }
    public string ShipmentReference { get; init; } = string.Empty;
    public string? PlataformaEnvioId { get; init; }
    public string? PlataformaEnvioEstado { get; init; }
    public int Bultos { get; init; }
    public bool EsDirecto { get; init; }
    public DestinoEnvioDto? Destino { get; init; }
    public IReadOnlyList<ExpedicionDto> Expediciones { get; init; } = [];
}

public sealed record DestinoEnvioDto
{
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string? CodigoPostal { get; init; }
    public string? Ciudad { get; init; }
    public string? CodigoPais { get; init; }
    public string? Telefono { get; init; }
}

public sealed record OrigenDto
{
    public string? WarehouseId { get; init; }
    public string? AddressName { get; init; }
    public string? AddressStreet { get; init; }
    public string? AddressPhone1 { get; init; }
    public string? Zipcode { get; init; }
    public string? City { get; init; }
    public string? ProvinceName { get; init; }
    public string? CountryName { get; init; }
    public string? CountryIsoCode { get; init; }
}

public sealed record ExpedicionDto
{
    public Guid Id { get; init; }
    public string ErpId { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public string? ExpeditionCode { get; init; }
    public int ExpeditionType { get; init; }
    public string Empresa { get; init; } = string.Empty;
    public Guid AlmacenId { get; init; }
    public Guid AgenciaId { get; init; }
    public DateOnly Fecha { get; init; }
    public string? Cliente { get; init; }
    public string? Pais { get; init; }
    public string? Provincia { get; init; }
    public string? CodigoPostal { get; init; }
    public string? Municipio { get; init; }
    public string? AlmacenDestino { get; init; }
    public int Bultos { get; init; }
    public Guid? EnvioId { get; init; }
}

public sealed record EventoDocumentoDto
{
    public Guid Id { get; init; }
    public DateTimeOffset Momento { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public int? EstadoHttp { get; init; }
    public string? Mensaje { get; init; }
}

public sealed record ConductorDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? TaxId { get; init; }
    public string? LicensePlate { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Movil { get; init; }
    public string Language { get; init; } = "es";
}

public sealed record CcDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Language { get; init; } = "es";
}
