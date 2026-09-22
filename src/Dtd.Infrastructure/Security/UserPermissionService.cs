using Dtd.Application.Common.Security;
using Dtd.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class UserPermissionService : IUserPermissionService
{
    private const string LegacyTokenHeader = "X-Legacy-Token";

    private const string WarehouseServiceName =
        "sga.dbo.HTML_Almacen";

    private const string CompanyServiceName =
        "sga.dbo.HTML_Empresa";

    private static readonly TimeSpan PermissionsCacheDuration =
        TimeSpan.FromMinutes(10);

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
        var username = GetUsername();

        var empresaNormalizada = empresa.Trim();

        var cacheKey =
            $"user-permissions:{empresaNormalizada}:{username}";

        if (_cache.TryGetValue(
            cacheKey,
            out IReadOnlyCollection<string>? cachedWarehouses))
        {
            return cachedWarehouses!;
        }

        var warehouses = await LoadWarehousesFromSgaAsync(
            empresaNormalizada,
            username,
            cancellationToken);

        _cache.Set(
            cacheKey,
            warehouses,
            PermissionsCacheDuration);

        return warehouses;
    }

    public async Task<IReadOnlyCollection<string>> GetAllowedCompaniesAsync(
        CancellationToken cancellationToken = default)
    {
        var username = GetUsername();

        var cacheKey =
            $"user-company-permissions:{username}";

        if (_cache.TryGetValue(
            cacheKey,
            out IReadOnlyCollection<string>? cachedCompanies))
        {
            return cachedCompanies!;
        }

        var companies = await LoadCompaniesFromSgaAsync(
            username,
            cancellationToken);

        _cache.Set(
            cacheKey,
            companies,
            PermissionsCacheDuration);

        return companies;
    }

    private async Task<IReadOnlyCollection<string>> LoadWarehousesFromSgaAsync(
    string empresa,
    string username,
    CancellationToken cancellationToken)
    {
        var legacyToken = GetLegacyToken();

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

        var raw = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(raw);

        var root = json.RootElement;

        if (!root.TryGetProperty("result", out var resultElement) ||
            !resultElement.TryGetInt32(out var resultCode))
        {
            throw new InvalidOperationException(
                $"SGA ha devuelto una respuesta no válida al consultar almacenes. Body: {raw}");
        }

        if (resultCode != 0)
        {
            var mensaje = GetSgaErrorMessage(root);

            if (resultCode == -99)
            {
                throw new UnauthorizedAccessException(
                    mensaje);
            }

            throw new InvalidOperationException(
                $"Error consultando almacenes permitidos en SGA. " +
                $"Result={resultCode}. {mensaje}");
        }

        var result = JsonSerializer.Deserialize<SgaWarehouseResponse>(
            raw,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result is null)
        {
            throw new InvalidOperationException(
                "El servicio SGA no ha devuelto una respuesta válida al consultar almacenes.");
        }

        return result.Value?.Rows
            .Select(r => r.Codigo.ToString())
            .ToList()
            ?? [];
    }
    private async Task<IReadOnlyCollection<string>> LoadCompaniesFromSgaAsync(
    string username,
    CancellationToken cancellationToken)
    {
        var legacyToken = GetLegacyToken();

        var request = new SgaCompanyRequest
        {
            Message = "execute",
            Usuario = username,
            AuthToken = legacyToken,
            ServiceName = CompanyServiceName
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "",
            request,
            cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(raw);

        var root = json.RootElement;

        if (!root.TryGetProperty("result", out var resultElement) ||
            !resultElement.TryGetInt32(out var resultCode))
        {
            throw new InvalidOperationException(
                $"SGA ha devuelto una respuesta no válida al consultar empresas. Body: {raw}");
        }

        if (resultCode != 0)
        {
            var mensaje = GetSgaErrorMessage(root);

            if (resultCode == -99)
            {
                throw new UnauthorizedAccessException(
                    mensaje);
            }

            throw new InvalidOperationException(
                $"Error consultando empresas permitidas en SGA. " +
                $"Result={resultCode}. {mensaje}");
        }

        var result = JsonSerializer.Deserialize<SgaCompanyResponse>(
            raw,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result is null)
        {
            throw new InvalidOperationException(
                "El servicio SGA no ha devuelto una respuesta válida al consultar empresas.");
        }

        return result.Value?.Rows
            .Select(r => r.Codigo.ToString("D3"))
            .ToList()
            ?? [];
    }

    private string GetLegacyToken()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "No existe HttpContext.");

        var legacyToken =
            httpContext.Request
                .Headers[LegacyTokenHeader]
                .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(legacyToken))
        {
            throw new UnauthorizedAccessException(
                $"No se ha recibido la cabecera '{LegacyTokenHeader}'.");
        }

        return legacyToken;
    }

    private static string GetSgaErrorMessage(
    JsonElement root)
    {
        if (!root.TryGetProperty("value", out var value))
        {
            return "SGA no ha proporcionado detalle del error.";
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString()
                ?? "SGA no ha proporcionado detalle del error.";
        }

        return value.GetRawText();
    }

    private string GetUsername()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "No existe HttpContext.");

        return
            httpContext.User
                .FindFirst("preferred_username")
                ?.Value
            ?? httpContext.User.Identity?.Name
            ?? throw new UnauthorizedAccessException(
                "Usuario no identificado.");
    }
}