using System.Text.Json.Serialization;

internal sealed class SgaWarehouseResponse
{
    [JsonPropertyName("result")]
    public int Result { get; init; }

    [JsonPropertyName("value")]
    public SgaWarehouseValue? Value { get; init; }
}

internal sealed class SgaWarehouseValue
{
    [JsonPropertyName("Rows")]
    public IReadOnlyCollection<SgaWarehouseRow> Rows { get; init; }
        = [];
}

internal sealed class SgaWarehouseRow
{
    [JsonPropertyName("CODIGO")]
    public int Codigo { get; init; }
}