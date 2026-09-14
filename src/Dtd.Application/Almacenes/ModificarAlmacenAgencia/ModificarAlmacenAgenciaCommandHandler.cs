using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Templates;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ModificarAlmacenAgencia;

internal sealed class ModificarAlmacenAgenciaCommandHandler
    : IRequestHandler<
        ModificarAlmacenAgenciaCommand,
        ErrorOr<AlmacenAgenciaDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ModificarAlmacenAgenciaCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        ITemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AlmacenAgenciaDto>> Handle(
        ModificarAlmacenAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null || almacen.Empresa != empresa)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                empresa,
                almacen.Id,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        var agencia = await _agenciaRepository.GetByIdConBasesAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var relacion =
            await _almacenRepository.GetRelacionAgenciaAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (relacion is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{agencia.Codigo}' no está vinculada " +
                $"al almacén '{almacen.Codigo}'.");
        }

        if (request.TemplateId == Guid.Empty)
        {
            return Error.Validation(
                "Template.Obligatorio",
                "El template es obligatorio.");
        }

        var template = await _templateRepository.GetByIdAsync(
            request.TemplateId,
            cancellationToken);

        if (template is null)
        {
            return Error.NotFound(
                "Template.NoEncontrado",
                $"No existe el template '{request.TemplateId}'.");
        }

        if (template.Empresa != empresa)
        {
            return Error.Validation(
                "Template.EmpresaNoValida",
                $"El template '{request.TemplateId}' no pertenece " +
                $"a la empresa '{empresa}'.");
        }

        if (!template.Active)
        {
            return Error.Validation(
                "Template.NoActivo",
                $"El template '{request.TemplateId}' no está activo.");
        }

        Guid? agenciaBaseId = request.AgenciaBaseId;

        if (agencia.EnvioDirecto)
        {
            if (agenciaBaseId is not null &&
                agenciaBaseId != Guid.Empty)
            {
                return Error.Validation(
                    "Almacen.AgenciaBaseNoPermitida",
                    $"La agencia '{agencia.Codigo}' es de envío directo " +
                    "y no puede tener una base configurada.");
            }

            agenciaBaseId = null;
        }
        else
        {
            if (agenciaBaseId is null ||
                agenciaBaseId == Guid.Empty)
            {
                return Error.Validation(
                    "Almacen.AgenciaBaseObligatoria",
                    $"La agencia '{agencia.Codigo}' no es de envío directo " +
                    "y requiere una base.");
            }

            var baseValida = agencia.Bases
                .Any(x => x.Id == agenciaBaseId.Value);

            if (!baseValida)
            {
                return Error.Validation(
                    "Almacen.AgenciaBaseNoValida",
                    $"La base '{agenciaBaseId}' no pertenece " +
                    $"a la agencia '{agencia.Codigo}'.");
            }
        }

        relacion.CambiarTemplate(template.Id);
        relacion.ConfigurarAgenciaBase(agenciaBaseId);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new AlmacenAgenciaDto(
            relacion.AlmacenId,
            relacion.AgenciaId,
            relacion.TemplateId,
            relacion.AgenciaBaseId);
    }
}