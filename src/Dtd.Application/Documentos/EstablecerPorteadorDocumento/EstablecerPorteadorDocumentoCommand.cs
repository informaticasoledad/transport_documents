using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EstablecerPorteadorDocumento;

public sealed record EstablecerPorteadorDocumentoCommand(
    Guid DocumentoId,
    string Nombre,
    string IdentificadorFiscal,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso)
    : IRequest<ErrorOr<Success>>;