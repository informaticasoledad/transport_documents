using Dtd.Domain.Agencias;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ObtenerAgencia;

internal sealed class ObtenerAgenciaQueryHandler
    : IRequestHandler<
        ObtenerAgenciaQuery,
        ErrorOr<AgenciaDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;

    public ObtenerAgenciaQueryHandler(
        IAgenciaRepository agenciaRepository)
    {
        _agenciaRepository = agenciaRepository;
    }

    public async Task<ErrorOr<AgenciaDto>> Handle(
        ObtenerAgenciaQuery request,
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

        return new AgenciaDto(
            agencia.Id,
            agencia.Codigo,
            agencia.Nombre,
            agencia.Activa,
            agencia.AgenciaQs,
            agencia.EnvioDirecto);
    }
}