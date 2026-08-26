using System.Text.Json.Serialization;

namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Una expedición tal como la devuelve el microservicio del ERP por HTTP. Es el contrato real
/// (ver <c>docs/expeditionDto.json</c>): estructura anidada con origen, destino y líneas de detalle.
/// </summary>
/// <remarks>
/// <see cref="Empresa"/> no viaja en el body del ERP: es parámetro de la query. El gateway lo
/// rellena con <c>with</c> tras la deserialización para que el dominio lo tenga disponible al
/// construir la <c>Expedicion</c>. El almacén y la agencia se reciben como <c>Id</c> (Guid) del
/// documento, no del DTO del ERP; el gateway ya no estampa el código de agencia.
/// </remarks>
public sealed record ExpedicionErpDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("documentNumber")]
    public string? DocumentNumber { get; init; }

    [JsonPropertyName("expeditionDate")]
    public DateTime ExpeditionDate { get; init; }

    [JsonPropertyName("expeditionCode")]
    public string? ExpeditionCode { get; init; }

    [JsonPropertyName("expeditionType")]
    public int ExpeditionType { get; init; }

    [JsonPropertyName("originWarehouseId")]
    public string? OriginWarehouseId { get; init; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; init; }

    [JsonPropertyName("destinationWarehouseId")]
    public string? DestinationWarehouseId { get; init; }

    [JsonPropertyName("expeditionOrigin")]
    public ExpeditionOriginErpDto? ExpeditionOrigin { get; init; }

    [JsonPropertyName("expeditionDestination")]
    public ExpeditionDestinationErpDto? ExpeditionDestination { get; init; }

    [JsonPropertyName("expeditionDetails")]
    public IReadOnlyList<ExpeditionDetailErpDto> ExpeditionDetails { get; init; } = [];

    /// <summary>No viene en el body del ERP; la rellena el gateway desde el parámetro de query.</summary>
    [JsonIgnore]
    public string Empresa { get; init; } = string.Empty;
}

public sealed record ExpeditionOriginErpDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("addressName")]
    public string? AddressName { get; init; }

    [JsonPropertyName("addressStreet")]
    public string? AddressStreet { get; init; }

    [JsonPropertyName("addressPhone1")]
    public string? AddressPhone1 { get; init; }

    [JsonPropertyName("zipcode")]
    public string? Zipcode { get; init; }

    [JsonPropertyName("city")]
    public string? City { get; init; }

    [JsonPropertyName("provinceName")]
    public string? ProvinceName { get; init; }

    [JsonPropertyName("countryName")]
    public string? CountryName { get; init; }

    [JsonPropertyName("countryIsoCode")]
    public string? CountryIsoCode { get; init; }
}

public sealed record ExpeditionDestinationErpDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("addressName")]
    public string? AddressName { get; init; }

    [JsonPropertyName("addressStreet")]
    public string? AddressStreet { get; init; }

    [JsonPropertyName("addressPhone1")]
    public string? AddressPhone1 { get; init; }

    [JsonPropertyName("zipcode")]
    public string? Zipcode { get; init; }

    [JsonPropertyName("city")]
    public string? City { get; init; }

    [JsonPropertyName("provinceName")]
    public string? ProvinceName { get; init; }

    [JsonPropertyName("countryName")]
    public string? CountryName { get; init; }

    [JsonPropertyName("countryIsoCode")]
    public string? CountryIsoCode { get; init; }
}

public sealed record ExpeditionDetailErpDto
{
    [JsonPropertyName("productId")]
    public string? ProductId { get; init; }

    [JsonPropertyName("productName")]
    public string? ProductName { get; init; }

    [JsonPropertyName("productUnits")]
    public decimal ProductUnits { get; init; }
}