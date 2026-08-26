using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>Actualiza un CC del catalogo y sustituye sus vinculos con relaciones almacen-agencia.</summary>
public sealed record ActualizarCcCommand(
    string Empresa,
    Guid CcId,
    string Nombre,
    string Email,
    string Language,
    IReadOnlyList<CcVinculoAlmacenAgenciaDto> Vinculos) : IRequest<ErrorOr<CcCatalogoDto>>;

internal sealed class ActualizarCcCommandValidator : AbstractValidator<ActualizarCcCommand>
{
    public ActualizarCcCommandValidator()
    {
        RuleFor(x => x.Empresa).NotEmpty();
        RuleFor(x => x.CcId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Vinculos).NotNull();
        RuleForEach(x => x.Vinculos).ChildRules(vinculo =>
        {
            vinculo.RuleFor(x => x.AlmacenId).NotEmpty();
            vinculo.RuleFor(x => x.AgenciaId).NotEmpty();
        });
    }
}

internal sealed class ActualizarCcCommandHandler
    : IRequestHandler<ActualizarCcCommand, ErrorOr<CcCatalogoDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public ActualizarCcCommandHandler(
        ICcRepository ccRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _ccRepository = ccRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<CcCatalogoDto>> Handle(ActualizarCcCommand request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var cc = await _ccRepository.GetByIdAsync(request.CcId, cancellationToken);
        if (cc is null)
        {
            return Error.NotFound(
                "Cc.NoEncontrado",
                $"No existe el CC '{request.CcId}'.");
        }

        if (cc.Empresa != empresa)
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El CC '{request.CcId}' no pertenece a la empresa '{empresa}'.");
        }

        var vinculos = request.Vinculos
            .Select(x => new CcVinculoAlmacenAgencia(x.AlmacenId, x.AgenciaId, x.PorDefecto))
            .ToList();

        var errorPertenencia = await ValidarRelacionesAsync(empresa, vinculos, cancellationToken);
        if (errorPertenencia is { } err)
        {
            return err;
        }

        try
        {
            var email = Email.Create(request.Email) ?? throw new ArgumentException("El email es obligatorio.", nameof(request.Email));
            cc.Actualizar(request.Nombre, email, request.Language);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation("Cc.DatosInvalidos", ex.Message);
        }

        await _ccRepository.ActualizarAsync(cc, vinculos, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CrearCcCommandHandler.ToDto(cc);
    }

    private async Task<Error?> ValidarRelacionesAsync(
        string empresa,
        IReadOnlyCollection<CcVinculoAlmacenAgencia> vinculos,
        CancellationToken cancellationToken)
    {
        if (vinculos.Count == 0)
        {
            return null;
        }

        var almacenIds = vinculos.Select(x => x.AlmacenId).Distinct().ToList();
        var agenciaIds = vinculos.Select(x => x.AgenciaId).Distinct().ToList();

        var almacenes = await _almacenRepository.GetByIdsAsync(almacenIds, cancellationToken);
        if (almacenes.Count != almacenIds.Count)
        {
            return Error.NotFound("Cc.AlmacenNoExiste", "Alguno de los almacenes indicados no existe.");
        }

        if (almacenes.Any(a => a.Empresa != empresa))
        {
            return Error.Forbidden("Cc.AlmacenOtraEmpresa", "Alguno de los almacenes indicados no pertenece a la empresa.");
        }

        var agencias = await _agenciaRepository.GetByIdsAsync(agenciaIds, cancellationToken);
        if (agencias.Count != agenciaIds.Count)
        {
            return Error.NotFound("Cc.AgenciaNoExiste", "Alguna de las agencias indicadas no existe.");
        }

        if (agencias.Any(a => a.Empresa != empresa))
        {
            return Error.Forbidden("Cc.AgenciaOtraEmpresa", "Alguna de las agencias indicadas no pertenece a la empresa.");
        }

        foreach (var vinculo in vinculos.DistinctBy(x => new { x.AlmacenId, x.AgenciaId }))
        {
            var disponible = await _almacenRepository.EsAgenciaDisponibleAsync(
                vinculo.AlmacenId,
                vinculo.AgenciaId,
                cancellationToken);

            if (!disponible)
            {
                return Error.NotFound(
                    "Cc.AlmacenAgenciaNoDisponible",
                    "Alguna de las relaciones almacen-agencia indicadas no existe.");
            }
        }

        return null;
    }
}
