using System.Text.Json.Serialization;

namespace Dtd.Application.GatewayContracts;

// --- DTOs de petición (contrato real POST /api/v1/lots, snake_case) ---
// 1 DDT = 1 lote; las expediciones del DDT son los shipments del lote. Los [JsonPropertyName]
// reflejan exactamente el contrato de Docuten (incluido el prefijo good_ de los bienes y los
// campos de coordenadas de firma). documents[] es opcional: en Fase 1 no se sube PDF (Content=null),
// por lo que el mapper deja Documents a null (lote JSON-only) hasta decidir el origen del PDF.

public sealed record DocutenLoteDto
{
    [JsonPropertyName("lot_reference")] public required string LotReference { get; init; }
    [JsonPropertyName("lot_name")] public required string LotName { get; init; }
    [JsonPropertyName("callback_url")] public string? CallbackUrl { get; init; }
    [JsonPropertyName("shipments")] public required IReadOnlyList<DocutenShipmentDto> Shipments { get; init; }
}

public sealed record DocutenShipmentDto
{
    [JsonPropertyName("shipment_reference")] public required string ShipmentReference { get; init; }
    [JsonPropertyName("shipment_name")] public required string ShipmentName { get; init; }
    [JsonPropertyName("callback_url")] public string? CallbackUrl { get; init; }
    [JsonPropertyName("language")] public required string Language { get; init; }
    [JsonPropertyName("origin")] public required DocutenOrigenDto Origin { get; init; }
    [JsonPropertyName("destination")] public required DocutenDestinoDto Destination { get; init; }
    [JsonPropertyName("parties")] public required DocutenPartiesDto Parties { get; init; }
    [JsonPropertyName("goods")] public required IReadOnlyList<DocutenGoodsDto> Goods { get; init; }
    [JsonPropertyName("documents")] public IReadOnlyList<DocutenDocumentoDto>? Documents { get; init; }
    [JsonPropertyName("metadata")] public required IReadOnlyList<DocutenMetadataDto> Metadata { get; init; }
}

public sealed record DocutenOrigenDto
{
    [JsonPropertyName("address")] public required string Address { get; init; }
    [JsonPropertyName("post_code")] public string? PostCode { get; init; }
    [JsonPropertyName("city")] public string? City { get; init; }
    [JsonPropertyName("country_code")] public string? CountryCode { get; init; }
}

public sealed record DocutenDestinoDto
{
    [JsonPropertyName("address")] public required string Address { get; init; }
    [JsonPropertyName("post_code")] public string? PostCode { get; init; }
    [JsonPropertyName("city")] public string? City { get; init; }
    [JsonPropertyName("country_code")] public string? CountryCode { get; init; }
}

public sealed record DocutenPartiesDto
{
    [JsonPropertyName("consignors")] public required IReadOnlyList<DocutenPartyDto> Consignors { get; init; }
    [JsonPropertyName("drivers")] public required IReadOnlyList<DocutenPartyDto> Drivers { get; init; }
    [JsonPropertyName("consignees")] public required IReadOnlyList<DocutenPartyDto> Consignees { get; init; }
}

/// <summary>Una party (consignor/driver/consignee) del shipment. <c>LicensePlate</c> sólo aplica al
/// driver; <c>Address</c>/<c>PostCode</c>/<c>City</c>/<c>CountryCode</c>/<c>SignerName</c>/
/// <c>SignerTaxId</c>/<c>RedirectUrl</c> los rellena hoy el consignor (driver/consignee los dejan a null).</summary>
public sealed record DocutenPartyDto
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("tax_id")] public string? TaxId { get; init; }
    [JsonPropertyName("license_plate")] public string? LicensePlate { get; init; }
    [JsonPropertyName("order")] public required int Order { get; init; }
    // signing_role/signature_type son opcionales: una party puede ser no-firmante (p.ej. el consignee
    // en Fase 1, sin contacto aún). Docuten sólo exige email/móvil "when signingRole is set", así que
    // null = no firma y no se exige contacto. WhenWritingNull los omite en el wire.
    [JsonPropertyName("signing_role")] public string? SigningRole { get; init; }
    [JsonPropertyName("signature_type")] public string? SignatureType { get; init; }
    [JsonPropertyName("channel")] public string? Channel { get; init; }
    [JsonPropertyName("email")] public string? Email { get; init; }
    [JsonPropertyName("mobile")] public string? Mobile { get; init; }
    [JsonPropertyName("language")] public string? Language { get; init; }
    [JsonPropertyName("signer_name")] public string? SignerName { get; init; }
    [JsonPropertyName("signer_tax_id")] public string? SignerTaxId { get; init; }
    [JsonPropertyName("address")] public string? Address { get; init; }
    [JsonPropertyName("post_code")] public string? PostCode { get; init; }
    [JsonPropertyName("city")] public string? City { get; init; }
    [JsonPropertyName("country_code")] public string? CountryCode { get; init; }
    [JsonPropertyName("redirect_url")] public string? RedirectUrl { get; init; }
}

public sealed record DocutenGoodsDto
{
    [JsonPropertyName("good_description")] public required string Description { get; init; }
    [JsonPropertyName("cargo_type")] public string? CargoType { get; init; }
    [JsonPropertyName("good_gross_mass")] public string? GrossMass { get; init; }
    [JsonPropertyName("dangerous_goods")] public bool? DangerousGoods { get; init; }
}

public sealed record DocutenDocumentoDto
{
    [JsonPropertyName("document_type")] public required string DocumentType { get; init; }
    [JsonPropertyName("document_name")] public required string DocumentName { get; init; }
    [JsonPropertyName("external_id")] public required string ExternalId { get; init; }
    /// <summary>PDF en Base64. Null en Fase 1 (PDF diferido); cuando se cablee el origen del PDF se rellena.</summary>
    [JsonPropertyName("content")] public string? Content { get; init; }
    [JsonPropertyName("signable")] public required bool Signable { get; init; }
    [JsonPropertyName("signers")] public IReadOnlyList<DocutenSignerDto>? Signers { get; init; }
}

public sealed record DocutenSignerDto
{
    [JsonPropertyName("order")] public required int Order { get; init; }
    [JsonPropertyName("coordinate")] public required DocutenSignerCoordinateDto Coordinate { get; init; }
}

public sealed record DocutenSignerCoordinateDto
{
    [JsonPropertyName("sig_page")] public int SigPage { get; init; }
    [JsonPropertyName("top_left_corner_x")] public int TopLeftCornerX { get; init; }
    [JsonPropertyName("top_left_corner_y")] public int TopLeftCornerY { get; init; }
    [JsonPropertyName("width")] public int Width { get; init; }
    [JsonPropertyName("height")] public int Height { get; init; }
}

public sealed record DocutenMetadataDto
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("value")] public required string Value { get; init; }
}

// --- DTOs de respuesta (wire-agnostic; el gateway los construye desde su DTO JSON interno) ---

/// <summary>Resultado de crear un lote (async): el id del lote y el estado inicial (típicamente "pending").</summary>
public sealed record DocutenLoteEnvioResult(
    string LotId,
    string Estado,
    IReadOnlyList<DocutenShipmentEnvioResult> Shipments);

public sealed record DocutenShipmentEnvioResult(
    string? ShipmentReference,
    string? ShipmentId,
    string ShipmentStatus);

/// <summary>Cuerpo de <c>POST /api/v1/shipments/{shipmentId}/cancel</c> (cancelación por shipment).</summary>
public sealed record DocutenCancelShipmentRequest
{
    [JsonPropertyName("reason")] public required string Reason { get; init; }
}

/// <summary>Estado sondeado de un lote: estado global + el estado de cada shipment.</summary>
public sealed record DocutenLoteEstadoResult(string Estado, IReadOnlyList<DocutenShipmentEstadoDto> Shipments);

public sealed record DocutenShipmentEstadoDto
{
    public string? ShipmentId { get; init; }
    public required string ShipmentStatus { get; init; }
    public string? SignatureStatus { get; init; }
    public string? ProofOfDelivery { get; init; }
}
