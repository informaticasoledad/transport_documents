using Dtd.Domain.Agencias;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ModificarAgencia;

internal sealed class ModificarAgenciaCommandHandler
    : IRequestHandler<
        ModificarAgenciaCommand,
        ErrorOr<AgenciaDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarAgenciaCommandHandler(
        IAgenciaRepository agenciaRepository,
        IUnitOfWork unitOfWork)
    {
        _agenciaRepository = agenciaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AgenciaDto>> Handle(
        ModificarAgenciaCommand request,
        CancellationToken cancellationToken)
    {
        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var codigo = request.Codigo.Trim();

        if (!string.Equals(
                agencia.Codigo,
                codigo,
                StringComparison.OrdinalIgnoreCase))
        {
            var existente = await _agenciaRepository.GetByCodigoAsync(
                codigo,
                cancellationToken);

            if (existente is not null &&
                existente.Id != agencia.Id)
            {
                return Error.Conflict(
                    "Agencia.CodigoDuplicado",
                    $"Ya existe una agencia con el código '{codigo}'.");
            }
        }

        try
        {
            agencia.Modificar(
                codigo,
                request.Nombre,
                request.AgenciaQs,
                request.EnvioDirecto);

            if (request.Activa)
            {
                agencia.Activar();
            }
            else
            {
                agencia.Desactivar();
            }
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Agencia.DatosInvalidos",
                ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AgenciaDto(
            agencia.Id,
            agencia.Codigo,
            agencia.Nombre,
            agencia.Activa,
            agencia.AgenciaQs,
            agencia.EnvioDirecto);
    }
}