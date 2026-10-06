using Dtd.Application.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>
/// Actualiza los datos de un CC del catálogo.
/// </summary>
public sealed record ActualizarCcCommand(
    string Empresa,
    Guid CcId,
    string Nombre,
    string Email,
    string Language)
    : IRequest<ErrorOr<CcCatalogoDto>>;

internal sealed class ActualizarCcCommandValidator
    : AbstractValidator<ActualizarCcCommand>
{
    public ActualizarCcCommandValidator()
    {
        RuleFor(x => x.Empresa)
            .NotEmpty();

        RuleFor(x => x.CcId)
            .NotEmpty();

        RuleFor(x => x.Nombre)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty();

        RuleFor(x => x.Language)
            .NotEmpty();
    }
}

internal sealed class ActualizarCcCommandHandler
    : IRequestHandler<ActualizarCcCommand, ErrorOr<CcCatalogoDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ActualizarCcCommandHandler(
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<CcCatalogoDto>> Handle(
        ActualizarCcCommand request,
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

        var cc = await _ccRepository.GetByIdAsync(
            request.CcId,
            cancellationToken);

        if (cc is null ||
            cc.Empresa != empresa)
        {
            return Error.NotFound(
                "Cc.NoEncontrado",
                $"No existe el CC '{request.CcId}' " +
                $"para la empresa '{empresa}'.");
        }

        try
        {
            var email = Email.Create(request.Email)
                ?? throw new ArgumentException(
                    "El email es obligatorio.",
                    nameof(request.Email));

            cc.Actualizar(
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

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearCcCommandHandler.ToDto(cc);
    }
}