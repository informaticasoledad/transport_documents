using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Application.Documentos;

internal static class DocumentoDtoFactory
{
    public static DocumentoDto ToDto(
        DocumentoDigitalTransporte documento,
        Almacen? almacen,
        Agencia? agencia)
    {
        var expediciones = documento.Expediciones
            .Select(ToDto)
            .ToList();

        return new DocumentoDto
        {
            Id = documento.Id,
            Empresa = documento.Empresa,
            AlmacenId = documento.AlmacenId,
            AlmacenCodigo = almacen?.Codigo ?? string.Empty,
            AlmacenNombre = almacen?.Nombre ?? string.Empty,
            AgenciaId = documento.AgenciaId,
            AgenciaCodigo = agencia?.Codigo ?? string.Empty,
            AgenciaNombre = agencia?.Nombre ?? string.Empty,
            Origen = ToDto(documento.Origen),
            FechaDesde = documento.RangoFechas.FechaDesde,
            FechaHasta = documento.RangoFechas.FechaHasta,
            Estado = documento.Estado.ToString(),
            PlataformaId = documento.PlataformaId,
            PlataformaEstado = documento.PlataformaEstado,
            Conductores = documento.Conductores.Select(ToDto).ToList(),
            Ccs = documento.Ccs.Select(ToDto).ToList(),
            Usuario = documento.UsuarioGeneracionId,
            FechaGeneracion = documento.FechaGeneracion,
            TotalExpediciones = documento.Expediciones.Count,
            Expediciones = expediciones,
            EnvioDirecto = documento.TipoAgrupacion == TipoAgrupacionEnvio.PorAlmacenDestino,
            Envios = documento.Envios
                .OrderBy(e => e.Orden)
                .Select(e => ToDto(e, expediciones))
                .ToList()
        };
    }

    private static EnvioDto ToDto(Envio envio, IReadOnlyList<ExpedicionDto> expediciones)
    {
        return new EnvioDto
        {
            Id = envio.Id,
            Orden = envio.Orden,
            ShipmentReference = envio.Referencia,
            PlataformaEnvioId = envio.PlataformaEnvioId,
            PlataformaEnvioEstado = envio.PlataformaEnvioEstado,
            Bultos = envio.Bultos,
            EsDirecto = expediciones.Any(e => e.EnvioId == envio.Id && !string.IsNullOrWhiteSpace(e.AlmacenDestino)),
            Destino = envio.Destino is null ? null : ToDto(envio.Destino),
            Expediciones = expediciones
                .Where(e => e.EnvioId == envio.Id)
                .ToList()
        };
    }

    private static DestinoEnvioDto ToDto(DestinoEnvio destino)
    {
        return new DestinoEnvioDto
        {
            Codigo = destino.Codigo,
            Nombre = destino.Nombre,
            Direccion = destino.Direccion,
            CodigoPostal = destino.CodigoPostal,
            Ciudad = destino.Ciudad,
            CodigoPais = destino.CodigoPais,
            Telefono = destino.Telefono
        };
    }

    private static OrigenDto ToDto(OrigenDocumento origen)
    {
        return new OrigenDto
        {
            WarehouseId = origen.WarehouseId,
            AddressName = origen.AddressName,
            AddressStreet = origen.AddressStreet,
            AddressPhone1 = origen.AddressPhone1,
            Zipcode = origen.Zipcode,
            City = origen.City,
            ProvinceName = origen.ProvinceName,
            CountryName = origen.CountryName,
            CountryIsoCode = origen.CountryIsoCode
        };
    }

    private static ExpedicionDto ToDto(Expedicion expedicion)
    {
        return new ExpedicionDto
        {
            Id = expedicion.Id,
            ErpId = expedicion.ErpId,
            DocumentNumber = expedicion.DocumentNumber,
            ExpeditionCode = expedicion.ExpeditionCode,
            ExpeditionType = expedicion.ExpeditionType,
            Empresa = expedicion.Empresa,
            AlmacenId = expedicion.AlmacenId,
            AgenciaId = expedicion.AgenciaId,
            Fecha = expedicion.Fecha,
            Cliente = expedicion.Cliente,
            Pais = expedicion.Destino.Pais,
            Provincia = expedicion.Destino.Provincia,
            CodigoPostal = expedicion.Destino.CodigoPostal,
            Municipio = expedicion.Destino.Municipio,
            AlmacenDestino = expedicion.Destino.AlmacenDestino,
            Bultos = expedicion.Bultos,
            EnvioId = expedicion.EnvioId
        };
    }

    private static ConductorDto ToDto(ConductorAsignado conductor)
    {
        return new ConductorDto
        {
            Id = conductor.Id,
            Codigo = conductor.ConductorCodigo,
            Nombre = conductor.Nombre,
            TaxId = conductor.TaxId,
            LicensePlate = conductor.LicensePlate,
            Channel = conductor.Canal.Valor,
            Email = conductor.Email?.Valor,
            Movil = conductor.Movil?.Valor,
            Language = conductor.Language
        };
    }

    private static CcDto ToDto(CcAsignado cc)
    {
        return new CcDto
        {
            Id = cc.Id,
            Codigo = cc.CcCodigo,
            Nombre = cc.Nombre,
            Email = cc.Email?.Valor,
            Language = cc.Language
        };
    }
}
