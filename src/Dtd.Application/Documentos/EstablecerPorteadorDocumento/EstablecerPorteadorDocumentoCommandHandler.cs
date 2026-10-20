using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EstablecerPorteadorDocumento;

internal sealed class EstablecerPorteadorDocumentoCommandHandler
    : IRequestHandler<
        EstablecerPorteadorDocumentoCommand,
        ErrorOr<Success>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EstablecerPorteadorDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IUnitOfWork unitOfWork)
    {
        _documentoRepository = documentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        EstablecerPorteadorDocumentoCommand request,
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

        Porteador porteador;

        try
        {
            porteador = Porteador.Crear(
                request.Nombre,
                request.IdentificadorFiscal,
                request.Direccion,
                request.CodigoPostal,
                request.Municipio,
                request.CodigoPaisIso);

            documento.EstablecerPorteador(porteador);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Documento.Porteador.DatosInvalidos",
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict(
                "Documento.Porteador.EstadoNoPermitido",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}