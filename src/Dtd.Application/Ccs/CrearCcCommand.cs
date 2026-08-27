using Dtd.Application.Almacenes;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Ccs;

public sealed record CcVinculoAlmacenAgenciaDto(
    Guid AlmacenId,
    Guid AgenciaId,
    bool PorDefecto);

/// <summary>
/// Crea un CC del catálogo de una empresa y lo vincula
/// a relaciones almacén-agencia concretas.
/// </summary>
public sealed record CrearCcCommand(
    string Empresa,
    string Codigo,
    string Nombre,
    string Email,
    string Language,
    IReadOnlyList<CcVinculoAlmacenAgenciaDto> Vinculos)
    : IRequest<ErrorOr<CcCatalogoDto>>;

internal sealed class CrearCcCommandValidator
    : AbstractValidator<CrearCcCommand>
{
    public CrearCcCommandValidator()
    {
        RuleFor(x => x.Empresa)
            .NotEmpty();

        RuleFor(x => x.Codigo)
            .NotEmpty();

        RuleFor(x => x.Nombre)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty();

        RuleFor(x => x.Vinculos)
            .NotEmpty()
            .WithMessage(
                "El CC debe vincularse al menos a una relación almacén-agencia.");

        RuleForEach(x => x.Vinculos)
            .ChildRules(vinculo =>
            {
                vinculo.RuleFor(x => x.AlmacenId)
                    .NotEmpty();

                vinculo.RuleFor(x => x.AgenciaId)
                    .NotEmpty();
            });
    }
}

internal sealed class CrearCcCommandHandler
    : IRequestHandler<CrearCcCommand, ErrorOr<CcCatalogoDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public CrearCcCommandHandler(
        ICcRepository ccRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<CcCatalogoDto>> Handle(
        CrearCcCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var accesoEmpresa =
            await _accesoAlmacenService.ValidarAccesoEmpresaAsync(
                empresa,
                cancellationToken);

        if (accesoEmpresa.IsError)
        {
            return accesoEmpresa.Errors;
        }

        var vinculos = request.Vinculos
            .Select(x => new CcVinculoAlmacenAgencia(
                x.AlmacenId,
                x.AgenciaId,
                x.PorDefecto))
            .ToList();

        var errorRelaciones =
            await ValidarRelacionesAsync(
                empresa,
                vinculos,
                cancellationToken);

        if (errorRelaciones is { } error)
        {
            return error;
        }

        var existente =
            await _ccRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.Codigo,
                cancellationToken);

        if (existente is not null)
        {
            return Error.Conflict(
                "Cc.YaExiste",
                $"Ya existe un CC con código '{request.Codigo}' " +
                $"en la empresa '{empresa}'.");
        }

        Cc cc;

        try
        {
            var email = Email.Create(request.Email)
                ?? throw new ArgumentException(
                    "El email es obligatorio.",
                    nameof(request.Email));

            cc = Cc.Crear(
                empresa,
                request.Codigo,
                request.Nombre,
                email,
                request.Language);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Cc.DatosInvalidos",
                ex.Message);
        }

        await _ccRepository.AddAsync(
            cc,
            vinculos,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToDto(cc);
    }

    internal async Task<Error?> ValidarRelacionesAsync(
        string empresa,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken)
    {
        var almacenIds = vinculos
            .Select(x => x.AlmacenId)
            .Distinct()
            .ToList();

        var agenciaIds = vinculos
            .Select(x => x.AgenciaId)
            .Distinct()
            .ToList();

        var almacenes =
            await _almacenRepository.GetByIdsAsync(
                almacenIds,
                cancellationToken);

        if (almacenes.Count != almacenIds.Count ||
            almacenes.Any(a => a.Empresa != empresa))
        {
            return Error.NotFound(
                "Cc.AlmacenNoExiste",
                "Alguno de los almacenes indicados no existe " +
                $"para la empresa '{empresa}'.");
        }

        var agencias =
            await _agenciaRepository.GetByIdsAsync(
                agenciaIds,
                cancellationToken);

        if (agencias.Count != agenciaIds.Count ||
            agencias.Any(a => a.Empresa != empresa))
        {
            return Error.NotFound(
                "Cc.AgenciaNoExiste",
                "Alguna de las agencias indicadas no existe " +
                $"para la empresa '{empresa}'.");
        }

        foreach (var vinculo in vinculos.DistinctBy(
                     x => new
                     {
                         x.AlmacenId,
                         x.AgenciaId
                     }))
        {
            var disponible =
                await _almacenRepository.EsAgenciaDisponibleAsync(
                    vinculo.AlmacenId,
                    vinculo.AgenciaId,
                    cancellationToken);

            if (!disponible)
            {
                return Error.NotFound(
                    "Cc.AlmacenAgenciaNoDisponible",
                    "Alguna de las relaciones almacén-agencia indicadas no existe.");
            }
        }

        return null;
    }

    internal static CcCatalogoDto ToDto(
        Cc c) => new()
        {
            Id = c.Id,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            Email = c.Email.Valor,
            Language = c.Language,
            Activo = c.Activo
        };
}