using Dtd.Application.Common.Security;
using ErrorOr;
using Microsoft.Extensions.Caching.Memory;

namespace Dtd.Application.Security;

internal sealed class ContextoAccesoService : IContextoAccesoService
{
    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromMinutes(30);

    private readonly IUsuarioContexto _usuarioContexto;
    private readonly IMemoryCache _cache;

    public ContextoAccesoService(
        IUsuarioContexto usuarioContexto,
        IMemoryCache cache)
    {
        _usuarioContexto = usuarioContexto;
        _cache = cache;
    }

    public Task<ErrorOr<ContextoAcceso>> ObtenerAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            return Task.FromResult<ErrorOr<ContextoAcceso>>(
                Error.Validation(
                    code: "Empresa.Requerida",
                    description: "La empresa es obligatoria."));
        }

        var usuario = _usuarioContexto.Current;

        if (usuario is null)
        {
            return Task.FromResult<ErrorOr<ContextoAcceso>>(
                Error.Unauthorized(
                    code: "Usuario.NoAutenticado",
                    description: "No se ha podido determinar el usuario autenticado."));
        }

        var empresaNormalizada = empresa.Trim();

        var cacheKey = GetCacheKey(
            usuario.Id,
            empresaNormalizada);

        if (_cache.TryGetValue<ContextoAcceso>(
                cacheKey,
                out var contextoCacheado) &&
            contextoCacheado is not null)
        {
            return Task.FromResult(
                contextoCacheado.ToErrorOr());
        }

        return Task.FromResult<ErrorOr<ContextoAcceso>>(
            Error.Unauthorized(
                code: "Acceso.ContextoNoInicializado",
                description:
                    $"No se ha cargado el contexto de acceso para la empresa '{empresaNormalizada}'."));
    }

    public Task GuardarAsync(
        ContextoAcceso contexto,
        CancellationToken cancellationToken = default)
    {
        var usuario = _usuarioContexto.Current;

        if (usuario is null)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(contexto.Empresa))
        {
            return Task.CompletedTask;
        }

        var empresaNormalizada =
            contexto.Empresa.Trim();

        var cacheKey = GetCacheKey(
            usuario.Id,
            empresaNormalizada);

        _cache.Set(
            cacheKey,
            contexto,
            CacheDuration);

        return Task.CompletedTask;
    }

    public Task EliminarAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        var usuario = _usuarioContexto.Current;

        if (usuario is null)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(empresa))
        {
            return Task.CompletedTask;
        }

        var empresaNormalizada = empresa.Trim();

        var cacheKey = GetCacheKey(
            usuario.Id,
            empresaNormalizada);

        _cache.Remove(cacheKey);

        return Task.CompletedTask;
    }

    private static string GetCacheKey(
        string usuarioId,
        string empresa)
    {
        return $"acceso:{usuarioId}:{empresa}";
    }
}