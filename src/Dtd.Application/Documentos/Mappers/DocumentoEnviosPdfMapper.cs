using Dtd.Application.Documentos.Pdf;
using Dtd.Domain.Documentos;

namespace Dtd.Application.Documentos.Mappers;

public static class DocumentoEnviosPdfMapper
{
    public static DocumentoEnviosPdfDto Map(
        DocumentoDigitalTransporte documento)
    {
        return new DocumentoEnviosPdfDto(
            DocumentoId: documento.Id,
            Empresa: documento.Empresa,
            Referencia: documento.Referencia,
            FechaCreacion: documento.FechaGeneracion.DateTime,
            Envios: documento.Envios
                .OrderBy(e => e.Orden)
                .Select(envio => MapEnvio(documento, envio))
                .ToList());
    }

    private static DocumentoEnvioPdfDto MapEnvio(
        DocumentoDigitalTransporte documento,
        Envio envio)
    {
        if (envio.Destino is null)
        {
            throw new InvalidOperationException(
                $"El envío '{envio.Referencia}' no tiene destino.");
        }

        var expediciones = documento.Expediciones
            .Where(e => e.EnvioId == envio.Id)
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.DocumentNumber)
            .ToList();

        return new DocumentoEnvioPdfDto(
            Referencia: envio.Referencia,

            Destino: envio.Destino.Nombre,
            Direccion: envio.Destino.Direccion,
            CodigoPostal: envio.Destino.CodigoPostal,
            Ciudad: envio.Destino.Ciudad,
            CodigoPais: envio.Destino.CodigoPais,

            TotalBultos: envio.Bultos,
            TotalPeso: envio.PesoTotal,

            Expediciones: expediciones
                .Select(MapExpedicion)
                .ToList());
    }
    private static DocumentoExpedicionPdfDto MapExpedicion(
        Expedicion expedicion)
    {
        return new DocumentoExpedicionPdfDto(
            Id: expedicion.ErpId,
            DocumentNumber: expedicion.DocumentNumber,
            Fecha: expedicion.Fecha.ToDateTime(TimeOnly.MinValue),
            Bultos: expedicion.Bultos,
            Peso: expedicion.PesoTotal);
    }
}