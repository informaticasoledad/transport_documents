using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// Dirección de origen del documento de transporte. Es común a todas las expediciones del DDT
/// (todas salen de la misma delegación/almacén de la agencia), por lo que vive a nivel de documento
/// y no por expedición. Proviene de <c>expeditionOrigin</c> del ERP.
/// </summary>
public sealed class OrigenDocumento : ValueObject
{
    /// <summary>Identificador del almacén/delegación de origen en el ERP (p. ej. "21").</summary>
    public string? WarehouseId { get; }

    public string? AddressName { get; }
    public string? AddressStreet { get; }
    public string? AddressPhone1 { get; }
    public string? Zipcode { get; }
    public string? City { get; }
    public string? ProvinceName { get; }
    public string? CountryName { get; }
    public string? CountryIsoCode { get; }

    private OrigenDocumento(
        string? warehouseId,
        string? addressName,
        string? addressStreet,
        string? addressPhone1,
        string? zipcode,
        string? city,
        string? provinceName,
        string? countryName,
        string? countryIsoCode)
    {
        WarehouseId = warehouseId;
        AddressName = addressName;
        AddressStreet = addressStreet;
        AddressPhone1 = addressPhone1;
        Zipcode = zipcode;
        City = city;
        ProvinceName = provinceName;
        CountryName = countryName;
        CountryIsoCode = countryIsoCode;
    }

    public static OrigenDocumento Create(
        string? warehouseId,
        string? addressName,
        string? addressStreet,
        string? addressPhone1,
        string? zipcode,
        string? city,
        string? provinceName,
        string? countryName,
        string? countryIsoCode) =>
        new(
            string.IsNullOrWhiteSpace(warehouseId) ? null : warehouseId.Trim(),
            string.IsNullOrWhiteSpace(addressName) ? null : addressName.Trim(),
            string.IsNullOrWhiteSpace(addressStreet) ? null : addressStreet.Trim(),
            string.IsNullOrWhiteSpace(addressPhone1) ? null : addressPhone1.Trim(),
            string.IsNullOrWhiteSpace(zipcode) ? null : zipcode.Trim(),
            string.IsNullOrWhiteSpace(city) ? null : city.Trim(),
            string.IsNullOrWhiteSpace(provinceName) ? null : provinceName.Trim(),
            string.IsNullOrWhiteSpace(countryName) ? null : countryName.Trim(),
            string.IsNullOrWhiteSpace(countryIsoCode) ? null : countryIsoCode.Trim());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return WarehouseId;
        yield return AddressName;
        yield return AddressStreet;
        yield return AddressPhone1;
        yield return Zipcode;
        yield return City;
        yield return ProvinceName;
        yield return CountryName;
        yield return CountryIsoCode;
    }
}