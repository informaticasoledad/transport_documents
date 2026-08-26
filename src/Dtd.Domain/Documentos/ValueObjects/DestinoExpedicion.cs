using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// Destination address of an expedition. <c>Pais</c> guarda el **ISO code** del país (ej. "ES"), no el
/// nombre legible — es lo que el ERP trae en <c>expeditionDestination.countryIsoCode</c> y lo que usa
/// Docuten como <c>country_code</c>; también permite inferir si el envío es nacional o internacional.
/// <c>AddressName</c>/<c>AddressStreet</c> alimentan el campo <c>address</c> del destino en Docuten.
/// </summary>
public sealed class DestinoExpedicion : ValueObject
{
    public string? Pais { get; }
    public string? Provincia { get; }
    public string? CodigoPostal { get; }
    public string? Municipio { get; }
    public string? AlmacenDestino { get; }
    public string? AddressName { get; }
    public string? AddressStreet { get; }
    /// <summary>Teléfono del destino (ERP <c>expeditionDestination.addressPhone1</c>).</summary>
    public string? AddressPhone1 { get; }

    private DestinoExpedicion(
        string? pais,
        string? provincia,
        string? codigoPostal,
        string? municipio,
        string? almacenDestino,
        string? addressName,
        string? addressStreet,
        string? addressPhone1)
    {
        Pais = string.IsNullOrWhiteSpace(pais) ? null : pais.Trim();
        Provincia = string.IsNullOrWhiteSpace(provincia) ? null : provincia.Trim();
        CodigoPostal = string.IsNullOrWhiteSpace(codigoPostal) ? null : codigoPostal.Trim();
        Municipio = string.IsNullOrWhiteSpace(municipio) ? null : municipio.Trim();
        AlmacenDestino = string.IsNullOrWhiteSpace(almacenDestino) ? null : almacenDestino.Trim();
        AddressName = string.IsNullOrWhiteSpace(addressName) ? null : addressName.Trim();
        AddressStreet = string.IsNullOrWhiteSpace(addressStreet) ? null : addressStreet.Trim();
        AddressPhone1 = string.IsNullOrWhiteSpace(addressPhone1) ? null : addressPhone1.Trim();
    }

    public static DestinoExpedicion Create(
        string? pais,
        string? provincia,
        string? codigoPostal,
        string? municipio,
        string? almacenDestino,
        string? addressName = null,
        string? addressStreet = null,
        string? addressPhone1 = null) =>
        new(pais, provincia, codigoPostal, municipio, almacenDestino, addressName, addressStreet, addressPhone1);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Pais;
        yield return Provincia;
        yield return CodigoPostal;
        yield return Municipio;
        yield return AlmacenDestino;
        yield return AddressName;
        yield return AddressStreet;
        yield return AddressPhone1;
    }
}
