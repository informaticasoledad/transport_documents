using Dtd.Application.Almacenes;
using Dtd.Application.Common.Security;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EliminarEnvio;

internal sealed class EliminarEnvioCommandHandler
    : IRequestHandler<
        EliminarEnvioCommand,
        ErrorOr<Deleted>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EliminarEnvioCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<Deleted>> Handle(
        EliminarEnvioCommand request,
        CancellationToken cancellationToken)
    {
        var documento =
            await _documentoRepository.GetByIdAsync(
                request.DocumentoId,
                cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                code: "Documento.NotFound",
                description:
                    $"No existe el documento '{request.DocumentoId}'.");
        }

        var acceso =
            await _accesoAlmacenService.ValidarAccesoAsync(
                documento.Empresa,
                documento.AlmacenId,
                cancellationToken);

        if (acceso.IsError)
        {
            return acceso.Errors;
        }

        var resultado =
            documento.EliminarEnvio(
                request.EnvioId);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Deleted;
    }
}