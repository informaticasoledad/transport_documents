using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ObtenerDocumento;

public sealed record ObtenerDocumentoQuery(
    Guid DocumentoId)
    : IRequest<ErrorOr<DocumentoDto>>;

internal sealed class ObtenerDocumentoQueryHandler
    : IRequestHandler<ObtenerDocumentoQuery, ErrorOr<DocumentoDto>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ObtenerDocumentoQueryHandler(
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<DocumentoDto>> Handle(
        ObtenerDocumentoQuery request,
        CancellationToken cancellationToken)
    {
        var documento =
            await _documentoRepository.GetByIdAsync(
                request.DocumentoId,
                cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                documento.Empresa,
                documento.AlmacenId,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        // El agregado sólo guarda los Ids (FK); el código/nombre del read model se resuelve
        // desde los maestros locales. No filtra por Activo.
        var almacen =
            await _almacenRepository.GetByIdAsync(
                documento.AlmacenId,
                cancellationToken);

        var agencia =
            await _agenciaRepository.GetByIdAsync(
                documento.AgenciaId,
                cancellationToken);

        return DocumentoDtoFactory.ToDto(
            documento,
            almacen,
            agencia);
    }
}