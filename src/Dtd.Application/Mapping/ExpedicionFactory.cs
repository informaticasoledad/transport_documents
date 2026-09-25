using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Application.Mapping;

/// <summary>
/// Construye entidades/VOs del dominio a partir del DTO de expedición del ERP.
/// </summary>
public static class ExpedicionFactory
{
    public static Expedicion ToDomain(
           this ExpedicionErpDto dto,
           Guid almacenId,
           Guid agenciaId)
    {
        var destino = DestinoExpedicion.Create(
            dto.ExpeditionDestination?.CountryIsoCode,
            dto.ExpeditionDestination?.ProvinceName,
            dto.ExpeditionDestination?.Zipcode,
            dto.ExpeditionDestination?.City,
            dto.ExpeditionDestination?.Id ?? dto.DestinationWarehouseId,
            dto.ExpeditionDestination?.AddressName,
            dto.ExpeditionDestination?.AddressStreet,
            dto.ExpeditionDestination?.AddressPhone1);

        var bultos = (int) dto.ExpeditionDetails
            .Sum(x => x.ProductUnits);

        var pesoTotal = dto.ExpeditionDetails
            .Sum(x => x.TotalWeight);

        return Expedicion.CrearDesdeErp(
            dto.Id,
            dto.DocumentNumber,
            dto.ExpeditionCode,
            dto.ExpeditionType,
            dto.Empresa,
            almacenId,
            agenciaId,
            DateOnly.FromDateTime(dto.ExpeditionDate),
            dto.CustomerId,
            destino,
            bultos,
            pesoTotal);
    }

    /// <summary>Construye el <see cref="OrigenDocumento"/> (común a todas las expediciones del DDT)
    /// desde el <c>expeditionOrigin</c> del DTO del ERP.</summary>
    public static OrigenDocumento ToOrigen(this ExpedicionErpDto dto)
    {
        var o = dto.ExpeditionOrigin;
        return OrigenDocumento.Create(
            o?.Id ?? dto.OriginWarehouseId,
            o?.AddressName,
            o?.AddressStreet,
            o?.AddressPhone1,
            o?.Zipcode,
            o?.City,
            o?.ProvinceName,
            o?.CountryName,
            o?.CountryIsoCode);
    }
}