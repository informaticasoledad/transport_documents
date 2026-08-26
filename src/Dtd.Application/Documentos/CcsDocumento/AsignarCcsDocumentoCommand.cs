using Dtd.Application.Documentos;
using Dtd.Application.Security;
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
/// Asigna uno o varios CCs del catálogo a un documento en estado <c>Nuevo</c>. Cada CC se identifica por
/// su <c>Id</c> (Guid) del catálogo y se snapshotea vía <see cref="CcAsignado.CrearDesdeCatalogo"/>. El
/// back verifica que cada CC esté vinculado (M:N) a AMBOS el almacén y la agencia del documento
/// (defense-in-depth, vía <see cref="ICcRepository.GetByAlmacenYAgenciaEIdAsync"/>) y que esté activo.
/// Idempotente por <c>CcCatalogId</c> (Ids repetidos o ya asignados no duplican). <b>All-or-nothing</b>:
/// si algún Id no existe, no está vinculado a ambos o está inactivo, no se asigna ninguno. Los CCs son
/// opcionales: el documento se puede enviar sin ninguno. Clon del handler de conductores.
/// </summary>
/// <returns>La lista de <see cref="CcDto"/> asignados (con su <c>Id</c> en el documento).</returns>
public sealed record AsignarCcsDocumentoCommand(Guid DocumentoId, IReadOnlyList<Guid> CcsId)
    : IRequest<ErrorOr<IReadOnlyList<CcDto>>>;

internal sealed class AsignarCcsDocumentoCommandValidator
    : AbstractValidator<AsignarCcsDocumentoCommand>
{
    public const int MaxCcs = 20;

    public AsignarCcsDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId).NotEmpty();
        RuleFor(x => x.CcsId)
            .NotEmpty().WithMessage("Debe indicar al menos un CC.")
            .Must(ids => ids.Count <= MaxCcs)
            .WithMessage(_ => $"No se pueden asignar más de {MaxCcs} CCs en una sola llamada.");
        RuleForEach(x => x.CcsId)
            .NotEmpty();
    }
}

internal sealed class AsignarCcsDocumentoCommandHandler
    : IRequestHandler<AsignarCcsDocumentoCommand, ErrorOr<IReadOnlyList<CcDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public AsignarCcsDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<CcDto>>> Handle(
        AsignarCcsDocumentoCommand request, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(request.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return Error.NotFound("Documento.NoEncontrado", $"No existe el documento '{request.DocumentoId}'.");
        }

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(documento.Empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{documento.Empresa}'.");
        }

        // Resolve el almacén/agencia del documento por Id (FK; RESTRICT garantiza que existen, pero
        // defense-in-depth: si se borrase el maestro dejando el doc huérfano, devolvemos 404 limpio).
        var almacen = await _almacenRepository.GetByIdAsync(documento.AlmacenId, cancellationToken);
        if (almacen is null)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{documento.AlmacenId}' del documento no existe.");
        }

        var agencia = await _agenciaRepository.GetByIdAsync(documento.AgenciaId, cancellationToken);
        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{documento.AgenciaId}' del documento no existe.");
        }

        // All-or-nothing: resuelve y valida TODOS los CCs antes de mutar el agregado, así un Id
        // inexistente, no vinculado a ambos (almacén+agencia) o inactivo no deja el documento parcialmente
        // mutado.
        if (request.CcsId.Any(id => id == Guid.Empty))
        {
            return Error.Validation(
                "Cc.IdRequerido",
                "Debe indicar un CC valido.");
        }

        var idsUnicos = request.CcsId
            .Distinct()
            .ToList();

        var resueltos = new List<Cc>(idsUnicos.Count);
        foreach (var id in idsUnicos)
        {
            var cc = await _ccRepository.GetByAlmacenYAgenciaEIdAsync(
                almacen.Id, agencia.Id, id, cancellationToken);
            if (cc is null)
            {
                return Error.NotFound(
                    "Cc.NoEncontrado",
                    $"No existe el CC '{id}' vinculado a ambos el almacén '{almacen.Codigo}' y la agencia '{agencia.Codigo}'.");
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
                documento.AsignarCc(CcAsignado.CrearDesdeCatalogo(cc));
            }
        }
        catch (InvalidOperationException ex)
        {
            // El documento ya no está en Nuevo (no se pueden añadir CCs).
            return Error.Conflict("Documento.YaConfirmado", ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Devuelve los CCs asignados que correspondan a los Ids pedidos (idempotente: AsignarCc descarta
        // duplicados por CcCatalogId, así no se devuelven extras).
        var pedidosIds = idsUnicos.ToHashSet();
        return documento.Ccs
            .Where(c => pedidosIds.Contains(c.CcCatalogId))
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
