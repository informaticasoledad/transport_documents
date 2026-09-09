using Dtd.Domain.Agencias;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Agencias.ListarAgenciaBases;

internal sealed class ListarAgenciaBasesQueryHandler
    : IRequestHandler<
        ListarAgenciaBasesQuery,
        ErrorOr<IReadOnlyList<AgenciaBaseDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;

    public ListarAgenciaBasesQueryHandler(
        IAgenciaRepository agenciaRepository)
    {
        _agenciaRepository = agenciaRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<AgenciaBaseDto>>> Handle(
        ListarAgenciaBasesQuery request,
        CancellationToken cancellationToken)
    {
        var agencia = await _agenciaRepository.GetByIdConBasesAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"No existe la agencia '{request.AgenciaId}'.");
        }

        return agencia.Bases
            .OrderBy(x => x.Nombre)
            .Select(x => new AgenciaBaseDto(
                x.Id,
                x.Codigo,
                x.Nombre,
                x.TaxId,
                x.Direccion,
                x.CodigoPostal,
                x.Municipio,
                x.CodigoPaisIso,
                x.Movil?.Valor,
                x.Email?.Valor,
                x.Canal.Valor,
                x.Language,
                x.Activo))
            .ToList();
    }
}