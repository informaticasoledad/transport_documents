using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Documentos.ConductoresDocumento;

/// <summary>
/// Asigna uno o varios conductores del catálogo de la agencia del documento
/// a un documento en estado <c>Nuevo</c>.
/// Cada conductor se identifica por su <c>Id</c> y se snapshotea mediante
/// <see cref="ConductorAsignado.CrearDesdeCatalogo"/>.
/// </summary>
public sealed record AsignarConductoresDocumentoCommand(
    Guid DocumentoId,
    IReadOnlyList<Guid> ConductoresId)
    : IRequest<ErrorOr<IReadOnlyList<ConductorDto>>>;

internal sealed class AsignarConductoresDocumentoCommandValidator
    : AbstractValidator<AsignarConductoresDocumentoCommand>
{
    public const int MaxConductores = 20;

    public AsignarConductoresDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId)
            .NotEmpty();

        RuleFor(x => x.ConductoresId)
            .NotEmpty()
            .WithMessage("Debe indicar al menos un conductor.")
            .Must(ids => ids.Count <= MaxConductores)
            .WithMessage(_ =>
                $"No se pueden asignar más de {MaxConductores} conductores en una sola llamada.");

        RuleForEach(x => x.ConductoresId)
            .NotEmpty();
    }
}

internal sealed class AsignarConductoresDocumentoCommandHandler
    : IRequestHandler<
        AsignarConductoresDocumentoCommand,
        ErrorOr<IReadOnlyList<ConductorDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public AsignarConductoresDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<ConductorDto>>> Handle(
        AsignarConductoresDocumentoCommand request,
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

        var almacen = await _almacenRepository.GetByIdAsync(
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

        var agencia = await _agenciaRepository.GetByIdAsync(
            documento.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{documento.AgenciaId}' del documento no existe.");
        }

        var agenciaDisponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (!agenciaDisponible)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{agencia.Codigo}' no está disponible " +
                $"para el almacén '{almacen.Codigo}'.");
        }

        if (request.ConductoresId.Any(id => id == Guid.Empty))
        {
            return Error.Validation(
                "Conductor.IdRequerido",
                "Debe indicar un conductor válido.");
        }

        var idsUnicos = request.ConductoresId
            .Distinct()
            .ToList();

        var resueltos = new List<Conductor>(idsUnicos.Count);

        foreach (var id in idsUnicos)
        {
            var conductor =
                await _conductorRepository.GetByAgenciaYIdAsync(
                    agencia.Id,
                    id,
                    cancellationToken);

            if (conductor is null)
            {
                return Error.NotFound(
                    "Conductor.NoEncontrado",
                    $"No existe el conductor '{id}' vinculado a la agencia " +
                    $"'{agencia.Codigo}'.");
            }

            if (!conductor.Activo)
            {
                return Error.Validation(
                    "Conductor.Inactivo",
                    $"El conductor '{conductor.Codigo}' está inactivo " +
                    "y no se puede asignar.");
            }

            resueltos.Add(conductor);
        }

        try
        {
            foreach (var conductor in resueltos)
            {
                documento.AsignarConductor(
                    ConductorAsignado.CrearDesdeCatalogo(conductor));
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

        return documento.Conductores
            .Where(c =>
                pedidosIds.Contains(c.ConductorCatalogId))
            .Select(c => new ConductorDto
            {
                Id = c.Id,
                Codigo = c.ConductorCodigo,
                Nombre = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Movil = c.Movil?.Valor,
                Language = c.Language
            })
            .ToList();
    }
}