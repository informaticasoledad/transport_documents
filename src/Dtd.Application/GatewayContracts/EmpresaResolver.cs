using Microsoft.Extensions.Caching.Memory;

namespace Dtd.Application.GatewayContracts;

/// <summary>
/// <see cref="IEmpresaResolver"/> con un cache-aside en memoria. Tanto los aciertos como los "no
/// configurados" (null) se cachean durante <see cref="_cacheTtl"/> para no machacar la base de datos;
/// una fila nueva/actualizada se pilla al expirar el TTL (o al reiniciar).
/// </summary>
internal sealed class EmpresaResolver : IEmpresaResolver
{
    private readonly IEmpresaRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTtl;

    public EmpresaResolver(IEmpresaRepository repository, IMemoryCache cache, TimeSpan cacheTtl)
    {
        _repository = repository;
        _cache = cache;
        _cacheTtl = cacheTtl;
    }

    public Task<EmpresaConfig?> ResolveAsync(string empresa, CancellationToken cancellationToken = default) =>
        _cache.GetOrCreateAsync(CacheKey(empresa), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheTtl;
            return _repository.GetByEmpresaAsync(empresa, cancellationToken);
        });

    private static string CacheKey(string empresa) => $"empresa:config:{empresa}";
}