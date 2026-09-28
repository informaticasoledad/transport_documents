using Dtd.Application.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ModificarPrecintoDocumento;

internal sealed class ModificarPrecintoDocumentoCommandHandler
    : IRequestHandler<
        ModificarPrecintoDocumentoCommand,
        ErrorOr<Success>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ModificarPrecintoDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<Success>> Handle(
        ModificarPrecintoDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(
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

        try
        {
            documento.ModificarPrecinto(request.Precinto);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict(
                "Documento.PrecintoNoModificable",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}