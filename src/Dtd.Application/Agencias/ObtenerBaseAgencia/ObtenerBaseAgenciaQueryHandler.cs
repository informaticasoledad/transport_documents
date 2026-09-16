using Dtd.Domain.Agencias;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ObtenerBaseAgencia;

internal sealed class ObtenerBaseAgenciaQueryHandler
    : IRequestHandler<
        ObtenerBaseAgenciaQuery,
        ErrorOr<AgenciaBaseDto>>
{
    private readonly IAgenciaRepository _agenciaRepository;

    public ObtenerBaseAgenciaQueryHandler(
        IAgenciaRepository agenciaRepository)
    {
        _agenciaRepository = agenciaRepository;
    }

    public async Task<ErrorOr<AgenciaBaseDto>> Handle(
        ObtenerBaseAgenciaQuery request,
        CancellationToken cancellationToken)
    {
        var agencia =
            await _agenciaRepository.GetByIdConBasesAsync(
                request.AgenciaId,
                cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        var baseAgencia = agencia.Bases
            .FirstOrDefault(b => b.Id == request.BaseId);

        if (baseAgencia is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrada",
                $"No existe la base '{request.BaseId}' en la agencia indicada.");
        }

        return new AgenciaBaseDto(
            baseAgencia.Id,
            baseAgencia.Codigo,
            baseAgencia.Nombre,
            baseAgencia.TaxId,
            baseAgencia.Direccion,
            baseAgencia.CodigoPostal,
            baseAgencia.Municipio,
            baseAgencia.CodigoPaisIso,
            baseAgencia.Movil?.Valor,
            baseAgencia.Email?.Valor,
            baseAgencia.Canal.Valor,
            baseAgencia.Language,
            baseAgencia.Activo);
    }
}