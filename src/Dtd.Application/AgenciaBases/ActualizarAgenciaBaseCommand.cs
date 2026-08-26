using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.AgenciaBases;

public sealed record ActualizarAgenciaBaseCommand(
    string Empresa,
    Guid AgenciaBaseId,
    string Nombre,
    string? TaxId,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso,
    string Canal,
    string? Movil,
    string? Email,
    string Language) : IRequest<ErrorOr<AgenciaBaseCatalogoDto>>;

internal sealed class ActualizarAgenciaBaseCommandValidator : AbstractValidator<ActualizarAgenciaBaseCommand>
{
    public ActualizarAgenciaBaseCommandValidator()
    {
        RuleFor(x => x.Empresa).NotEmpty();
        RuleFor(x => x.AgenciaBaseId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Canal).NotEmpty();
        RuleFor(x => x).Must(x => CrearAgenciaBaseCommandValidator.CanalContactoCoherente(x.Canal, x.Email, x.Movil))
            .WithMessage(x => $"El canal '{x.Canal}' requiere un contacto coherente (email->Email; sms/whatsapp->Movil).");
    }
}

internal sealed class ActualizarAgenciaBaseCommandHandler
    : IRequestHandler<ActualizarAgenciaBaseCommand, ErrorOr<AgenciaBaseCatalogoDto>>
{
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public ActualizarAgenciaBaseCommandHandler(
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<AgenciaBaseCatalogoDto>> Handle(
        ActualizarAgenciaBaseCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agenciaBase = await _agenciaBaseRepository.GetByIdAsync(request.AgenciaBaseId, cancellationToken);
        if (agenciaBase is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrado",
                $"No existe el agenciaBase '{request.AgenciaBaseId}'.");
        }

        if (agenciaBase.Empresa != empresa)
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El agenciaBase '{request.AgenciaBaseId}' no pertenece a la empresa '{empresa}'.");
        }

        try
        {
            var canal = Canal.Create(request.Canal)
                ?? throw new ArgumentException("El canal es obligatorio.", nameof(request.Canal));
            var movil = Movil.Create(request.Movil);
            var email = Email.Create(request.Email);

            agenciaBase.Actualizar(
                request.Nombre,
                request.TaxId,
                canal,
                movil,
                email,
                request.Language,
                request.Direccion,
                request.CodigoPostal,
                request.Municipio,
                request.CodigoPaisIso);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation("AgenciaBase.DatosInvalidos", ex.Message);
        }

        await _agenciaBaseRepository.ActualizarAsync(agenciaBase, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CrearAgenciaBaseCommandHandler.ToDto(agenciaBase);
    }
}
