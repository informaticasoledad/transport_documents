using Dtd.Application.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>
/// Crea un CC del catálogo de una empresa.
/// </summary>
public sealed record CrearCcCommand(
    string Empresa,
    string Codigo,
    string Nombre,
    string Email,
    string Language)
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

        RuleFor(x => x.Language)
            .NotEmpty();
    }
}

internal sealed class CrearCcCommandHandler
    : IRequestHandler<CrearCcCommand, ErrorOr<CcCatalogoDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public CrearCcCommandHandler(
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
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
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToDto(cc);
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