using Dtd.Application.Almacenes;
using Dtd.Domain.Almacenes;
using ErrorOr;
using Microsoft.Extensions.Caching.Memory;

namespace Dtd.Application.Security;

internal sealed class ContextoAccesoService : IContextoAccesoService
{
    private readonly IUsuarioContexto _usuarioContexto;
    private readonly IUsuarioAlmacenesProvider _usuarioAlmacenesProvider;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IMemoryCache _cache;

    public ContextoAccesoService(
        IUsuarioContexto usuarioContexto,
        IUsuarioAlmacenesProvider usuarioAlmacenesProvider,
        IAlmacenRepository almacenRepository,
        IMemoryCache cache)
    {
        _usuarioContexto = usuarioContexto;
        _usuarioAlmacenesProvider = usuarioAlmacenesProvider;
        _almacenRepository = almacenRepository;
        _cache = cache;
    }

    public async Task<ErrorOr<ContextoAcceso>> ObtenerAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            return Error.Validation(
                "Empresa.Requerida",
                "La empresa es obligatoria.");
        }

        var usuario = _usuarioContexto.Current;

        if (usuario is null)
        {
            return Error.Unauthorized(
                "Usuario.NoAutenticado",
                "No se ha podido determinar el usuario autenticado.");
        }

        var empresaNormalizada = empresa.Trim();
        var cacheKey = $"acceso:{usuario.Id}:{empresaNormalizada}";

        if (_cache.TryGetValue<ContextoAcceso>(cacheKey, out var contextoCacheado) &&
            contextoCacheado is not null)
        {
            return contextoCacheado.ToErrorOr();
        }

        var codigosPermitidos =
            await _usuarioAlmacenesProvider.ObtenerAlmacenesPermitidosAsync(
                usuario.Username,
                empresaNormalizada,
                cancellationToken);

        if (codigosPermitidos.Count == 0)
        {
            return Error.Forbidden(
                "Almacen.SinAcceso",
                $"El usuario no tiene almacenes autorizados para la empresa '{empresaNormalizada}'.");
        }

        var almacenes = await _almacenRepository.ObtenerPorCodigosAsync(
            empresaNormalizada,
            codigosPermitidos,
            cancellationToken);

        var ids = almacenes
            .Select(a => a.Id)
            .ToList();

        if (ids.Count == 0)
        {
            return Error.Forbidden(
                "Almacen.SinAcceso",
                $"No se han encontrado almacenes autorizados para la empresa '{empresaNormalizada}'.");
        }

        var contexto = new ContextoAcceso(
            empresaNormalizada,
            ids);

        _cache.Set(
            cacheKey,
            contexto,
            TimeSpan.FromMinutes(30));

        return contexto.ToErrorOr();
    }
}