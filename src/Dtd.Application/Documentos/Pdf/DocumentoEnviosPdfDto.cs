namespace Dtd.Application.Documentos.Pdf;

public sealed record DocumentoEnviosPdfDto(
    Guid DocumentoId,
    string Empresa,
    string Referencia,
    DateTime FechaCreacion,
    IReadOnlyCollection<DocumentoEnvioPdfDto> Envios);

public sealed record DocumentoEnvioPdfDto(
    string Referencia,
    string Destino,
    string Direccion,
    string? CodigoPostal,
    string? Ciudad,
    string? CodigoPais,
    int TotalBultos,
    decimal TotalPeso,
    IReadOnlyCollection<DocumentoExpedicionPdfDto> Expediciones);

public sealed record DocumentoExpedicionPdfDto(
    string Id,
    string? DocumentNumber,
    DateTime Fecha,
    int Bultos,
    decimal Peso);