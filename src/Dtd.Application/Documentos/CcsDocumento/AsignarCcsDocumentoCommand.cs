using Dtd.Application.Almacenes;
using Dtd.Application.Documentos;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Documentos.CcsDocumento;

/// <summary>
/// Asigna uno o varios CCs del catálogo a un documento en estado <c>Nuevo</c>.
/// Cada CC se identifica por su <c>Id</c> (Guid) del catálogo y se snapshotea vía
/// <see cref="CcAsignado.CrearDesdeCatalogo"/>.
/// El back verifica que cada CC esté vinculado al almacén y agencia del documento
/// y que esté activo.
/// </summary>
public sealed record AsignarCcsDocumentoCommand(
    Guid DocumentoId,
    IReadOnlyList<Guid> CcsId)
    : IRequest<ErrorOr<IReadOnlyList<CcDto>>>;

internal sealed class AsignarCcsDocumentoCommandValidator
    : AbstractValidator<AsignarCcsDocumentoCommand>
{
    public const int MaxCcs = 20;

    public AsignarCcsDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId)
            .NotEmpty();

        RuleFor(x => x.CcsId)
            .NotEmpty()
            .WithMessage("Debe indicar al menos un CC.")
            .Must(ids => ids.Count <= MaxCcs)
            .WithMessage(_ =>
                $"No se pueden asignar más de {MaxCcs} CCs en una sola llamada.");

        RuleForEach(x => x.CcsId)
            .NotEmpty();
    }
}

internal sealed class AsignarCcsDocumentoCommandHandler
    : IRequestHandler<
        AsignarCcsDocumentoCommand,
        ErrorOr<IReadOnlyList<CcDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public AsignarCcsDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<CcDto>>> Handle(
        AsignarCcsDocumentoCommand request,
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

        var almacen =
            await _almacenRepository.GetByIdAsync(
                documento.AlmacenId,
                cancellationToken);

        if (almacen is null ||
            almacen.Empresa != documento.Empresa)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{documento.AlmacenId}' del documento " +
                $"no existe para la empresa '{documento.Empresa}'.");
        }

        var agencia =
            await _agenciaRepository.GetByIdAsync(
                documento.AgenciaId,
                cancellationToken);

        if (agencia is null ||
            agencia.Empresa != documento.Empresa)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{documento.AgenciaId}' del documento " +
                $"no existe para la empresa '{documento.Empresa}'.");
        }

        if (request.CcsId.Any(id => id == Guid.Empty))
        {
            return Error.Validation(
                "Cc.IdRequerido",
                "Debe indicar un CC válido.");
        }

        var idsUnicos = request.CcsId
            .Distinct()
            .ToList();

        var resueltos = new List<Cc>(idsUnicos.Count);

        foreach (var id in idsUnicos)
        {
            var cc =
                await _ccRepository.GetByAlmacenYAgenciaEIdAsync(
                    almacen.Id,
                    agencia.Id,
                    id,
                    cancellationToken);

            if (cc is null)
            {
                return Error.NotFound(
                    "Cc.NoEncontrado",
                    $"No existe el CC '{id}' vinculado al almacén " +
                    $"'{almacen.Codigo}' y la agencia '{agencia.Codigo}'.");
            }

            if (!cc.Activo)
            {
                return Error.Validation(
                    "Cc.Inactivo",
                    $"El CC '{cc.Codigo}' está inactivo y no se puede asignar.");
            }

            resueltos.Add(cc);
        }

        try
        {
            foreach (var cc in resueltos)
            {
                documento.AsignarCc(
                    CcAsignado.CrearDesdeCatalogo(cc));
            }
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict(
                "Documento.YaConfirmado",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var pedidosIds = idsUnicos.ToHashSet();

        return documento.Ccs
            .Where(c =>
                pedidosIds.Contains(c.CcCatalogId))
            .Select(c => new CcDto
            {
                Id = c.Id,
                Codigo = c.CcCodigo,
                Nombre = c.Nombre,
                Email = c.Email?.Valor,
                Language = c.Language
            })
            .ToList();
    }
}