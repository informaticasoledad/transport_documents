using Dtd.Application.AgenciaBases;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.AgenciaBases;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.EstablecerAgenciaBase;

public sealed record EstablecerAgenciaBaseCommand(
    string Empresa,
    string AlmacenCodigo,
    string AgenciaCodigo,
    Guid AgenciaBaseId) : IRequest<ErrorOr<AgenciaBaseCatalogoDto>>;

internal sealed class EstablecerAgenciaBaseCommandHandler
    : IRequestHandler<EstablecerAgenciaBaseCommand, ErrorOr<AgenciaBaseCatalogoDto>>
{
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public EstablecerAgenciaBaseCommandHandler(
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<AgenciaBaseCatalogoDto>> Handle(
        EstablecerAgenciaBaseCommand request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario &&
            !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        if (request.AgenciaBaseId == Guid.Empty)
        {
            return Error.Validation(
                "AgenciaBase.IdRequerido",
                "El agencia base es obligatorio.");
        }

        var almacen = await _almacenRepository.GetByEmpresaYCodigoAsync(
            empresa,
            request.AlmacenCodigo,
            cancellationToken);

        if (almacen is null)
        {
            return Error.NotFound(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenCodigo}' no existe para la empresa '{empresa}'.");
        }

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(
            empresa,
            request.AgenciaCodigo,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoConfigurada",
                $"La agencia '{request.AgenciaCodigo}' no existe para la empresa '{empresa}'.");
        }

        if (agencia.EnvioDirecto)
        {
            return Error.Validation(
                "AlmacenAgencia.AgenciaBaseNoPermitido",
                $"La agencia '{agencia.Codigo}' agrupa por almacén destino y no admite agencia base.");
        }

        var relacion = await _almacenRepository.GetRelacionAgenciaParaActualizarAsync(
            almacen.Id,
            agencia.Id,
            cancellationToken);

        if (relacion is null)
        {
            return Error.NotFound(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{request.AgenciaCodigo}' no está disponible para el almacén '{request.AlmacenCodigo}' (empresa '{empresa}').");
        }

        var agenciaBase = await _agenciaBaseRepository.GetByIdAsync(
            request.AgenciaBaseId,
            cancellationToken);

        if (agenciaBase is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrado",
                $"No existe el agenciaBase '{request.AgenciaBaseId}'.");
        }

        if (agenciaBase.Empresa != empresa)
        {
            return Error.Forbidden(
                "AgenciaBase.OtraEmpresa",
                $"El agenciaBase '{request.AgenciaBaseId}' no pertenece a la empresa '{empresa}'.");
        }

        if (!agenciaBase.Activo)
        {
            return Error.Validation(
                "AgenciaBase.Inactivo",
                $"El agenciaBase '{agenciaBase.Codigo}' no está activo.");
        }

        if (!agenciaBase.TieneDireccionCompleta)
        {
            return Error.Validation(
                "AgenciaBase.SinDireccionBase",
                $"El agenciaBase '{agenciaBase.Codigo}' no tiene dirección completa para usarlo como base.");
        }

        relacion.ConfigurarAgenciaBase(agenciaBase.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CrearAgenciaBaseCommandHandler.ToDto(agenciaBase);
    }
}
