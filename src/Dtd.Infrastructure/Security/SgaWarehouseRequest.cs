using System.Text.Json.Serialization;

internal sealed class SgaWarehouseRequest
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;

    [JsonPropertyName("EMPRESA")]
    public int Empresa { get; init; }

    [JsonPropertyName("USRPGM")]
    public string Usuario { get; init; } = default!;

    [JsonPropertyName("authToken")]
    public string AuthToken { get; init; } = default!;

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; init; } = default!;
}