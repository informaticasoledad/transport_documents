using Dtd.Application.Ccs;
using Dtd.Domain.Almacenes;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ListarCcsPorAlmacenAgencia;

internal sealed class ListarCcsPorAlmacenAgenciaQueryHandler
    : IRequestHandler<
        ListarCcsPorAlmacenAgenciaQuery,
        ErrorOr<IReadOnlyList<CcAlmacenAgenciaDto>>>
{
    private readonly IAlmacenRepository _almacenRepository;

    public ListarCcsPorAlmacenAgenciaQueryHandler(
        IAlmacenRepository almacenRepository)
    {
        _almacenRepository = almacenRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<CcAlmacenAgenciaDto>>> Handle(
        ListarCcsPorAlmacenAgenciaQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null ||
            !string.Equals(
                almacen.Empresa,
                empresa,
                StringComparison.OrdinalIgnoreCase))
        {
            return Error.NotFound(
                code: "Almacen.NoEncontrado",
                description: "El almacén no existe.");
        }

        var almacenAgencia =
            await _almacenRepository.GetRelacionAgenciaAsync(
                request.AlmacenId,
                request.AgenciaId,
                cancellationToken);

        if (almacenAgencia is null)
        {
            return Error.NotFound(
                code: "AlmacenAgencia.NoEncontrada",
                description: "La agencia no está vinculada al almacén.");
        }

        var ccs = await _almacenRepository.ListarCcsPorAlmacenAgenciaAsync(
            request.AlmacenId,
            request.AgenciaId,
            cancellationToken);

        IReadOnlyList<CcAlmacenAgenciaDto> result = ccs
            .Select(vinculo => new CcAlmacenAgenciaDto(
                vinculo.Cc.Id,
                vinculo.Cc.Codigo,
                vinculo.Cc.Nombre,
                vinculo.Cc.Email.ToString(),
                vinculo.Cc.Language,
                vinculo.Cc.Activo,
                vinculo.PorDefecto))
            .ToList();

        return ErrorOrFactory.From<IReadOnlyList<CcAlmacenAgenciaDto>>(result);
    }
}