using Dtd.Application.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.AgenciaBases;

public sealed record CrearAgenciaBaseCommand(
    string Empresa,
    string Codigo,
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso,
    string Language)
    : IRequest<ErrorOr<AgenciaBaseCatalogoDto>>;

internal sealed class CrearAgenciaBaseCommandValidator
    : AbstractValidator<CrearAgenciaBaseCommand>
{
    public CrearAgenciaBaseCommandValidator()
    {
        RuleFor(x => x.Empresa)
            .NotEmpty();

        RuleFor(x => x.Codigo)
            .NotEmpty();

        RuleFor(x => x.Nombre)
            .NotEmpty();

        RuleFor(x => x.Canal)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x =>
                CanalContactoCoherente(
                    x.Canal,
                    x.Email,
                    x.Movil))
            .WithMessage(x =>
                $"El canal '{x.Canal}' requiere un contacto coherente " +
                "(email->Email; sms/whatsapp->Movil).");
    }

    internal static bool CanalContactoCoherente(
        string canal,
        string? email,
        string? movil)
    {
        var c = (canal ?? string.Empty)
            .Trim()
            .ToLowerInvariant();

        if (c == Canal.Email)
        {
            return !string.IsNullOrWhiteSpace(email);
        }

        if (c == Canal.Sms ||
            c == Canal.Whatsapp)
        {
            return !string.IsNullOrWhiteSpace(movil);
        }

        return false;
    }
}

internal sealed class CrearAgenciaBaseCommandHandler
    : IRequestHandler<
        CrearAgenciaBaseCommand,
        ErrorOr<AgenciaBaseCatalogoDto>>
{
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public CrearAgenciaBaseCommandHandler(
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AgenciaBaseCatalogoDto>> Handle(
        CrearAgenciaBaseCommand request,
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
            await _agenciaBaseRepository.GetByEmpresaYCodigoAsync(
                empresa,
                request.Codigo,
                cancellationToken);

        if (existente is not null)
        {
            return Error.Conflict(
                "AgenciaBase.YaExiste",
                $"Ya existe un agenciaBase con codigo " +
                $"'{request.Codigo}' en la empresa '{empresa}'.");
        }

        AgenciaBase agenciaBase;

        try
        {
            var canal = Canal.Create(request.Canal)
                ?? throw new ArgumentException(
                    "El canal es obligatorio.",
                    nameof(request.Canal));

            var movil = Movil.Create(request.Movil);
            var email = Email.Create(request.Email);

            agenciaBase = AgenciaBase.Crear(
                empresa,
                request.Codigo,
                request.Nombre,
                canal,
                movil,
                email,
                request.TaxId,
                request.Language,
                request.Direccion,
                request.CodigoPostal,
                request.Municipio,
                request.CodigoPaisIso);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "AgenciaBase.DatosInvalidos",
                ex.Message);
        }

        await _agenciaBaseRepository.AddAsync(
            agenciaBase,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToDto(agenciaBase);
    }

    internal static AgenciaBaseCatalogoDto ToDto(
        AgenciaBase c) => new()
        {
            Id = c.Id,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            TaxId = c.TaxId,
            Direccion = c.Direccion,
            CodigoPostal = c.CodigoPostal,
            Municipio = c.Municipio,
            CodigoPaisIso = c.CodigoPaisIso,
            Channel = c.Canal.Valor,
            Email = c.Email?.Valor,
            Movil = c.Movil?.Valor,
            Language = c.Language,
            Activo = c.Activo
        };
}