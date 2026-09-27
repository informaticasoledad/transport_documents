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

        var bultos = (int)dto.ExpeditionDetails
            .Sum(x => x.ProductUnits);

        var pesoTotal = dto.ExpeditionDetails
            .Sum(x => x.TotalWeight);

        var tipoExpedicion = dto.ExpeditionType switch
        {
            1 => TipoExpedicion.VentaCliente,
            2 => TipoExpedicion.TrasiegoAlmacen,
            3 => TipoExpedicion.TrasiegoAutomaticoAlmacen,
            4 => TipoExpedicion.CustodiaCliente,
            5 => TipoExpedicion.SalidaDepositoCliente,

            _ => throw new InvalidOperationException(
                $"Tipo de expedición ERP no soportado: {dto.ExpeditionType}.")
        };

        return Expedicion.CrearDesdeErp(
            dto.Id,
            dto.DocumentNumber,
            dto.ExpeditionCode,
            tipoExpedicion,
            dto.Empresa,
            almacenId,
            agenciaId,
            DateOnly.FromDateTime(dto.ExpeditionDate),
            dto.CustomerId,
            destino,
            bultos,
            pesoTotal);
    }

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