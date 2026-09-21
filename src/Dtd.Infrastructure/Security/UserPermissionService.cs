using Dtd.Application.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

internal sealed class UserPermissionService : IUserPermissionService
{
    private const string LegacyTokenHeader = "X-Legacy-Token";
    private const string WarehouseServiceName = "sga.dbo.HTML_Almacen";

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;

    public UserPermissionService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<string>> GetAllowedWarehousesAsync(
        string empresa,
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No existe HttpContext.");
        

        var username =
            httpContext.User.FindFirst("preferred_username")?.Value
            ?? httpContext.User.Identity?.Name
            ?? throw new UnauthorizedAccessException("Usuario no identificado.");


        var cacheKey = $"user-permissions:{empresa}:{username}";

        if (_cache.TryGetValue(
            cacheKey,
            out IReadOnlyCollection<string>? cachedWarehouses))
        {
            return cachedWarehouses!;
        }

        var warehouses = await LoadFromSgaAsync(
            empresa,
            username,
            cancellationToken);

        _cache.Set(
            cacheKey,
            warehouses,
            TimeSpan.FromMinutes(10));

        return warehouses;
    }

    private async Task<IReadOnlyCollection<string>> LoadFromSgaAsync(
        string empresa,
        string username,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No existe HttpContext.");

        var legacyToken =
            httpContext.Request.Headers[LegacyTokenHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(legacyToken))
        {
            throw new UnauthorizedAccessException(
                $"No se ha recibido la cabecera '{LegacyTokenHeader}'.");
        }

        if (!int.TryParse(empresa, out var empresaSga))
        {
            throw new ArgumentException(
                $"La empresa '{empresa}' no es válida.",
                nameof(empresa));
        }

        var request = new SgaWarehouseRequest
        {
            Message = "execute",
            Empresa = empresaSga,
            Usuario = username,
            AuthToken = legacyToken,
            ServiceName = WarehouseServiceName
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<SgaWarehouseResponse>(
                cancellationToken: cancellationToken);

        if (result is null)
        {
            return [];
        }

        return result.Value?.Rows.Select(r => r.Codigo.ToString()).ToList() ?? [];
    }
}