using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dtd.Infrastructure.Security
{
    internal sealed class SgaCompanyRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = default!;

        [JsonPropertyName("USRPGM")]
        public string Usuario { get; init; } = default!;

        [JsonPropertyName("authToken")]
        public string AuthToken { get; init; } = default!;

        [JsonPropertyName("serviceName")]
        public string ServiceName { get; init; } = default!;
    }

    internal sealed class SgaCompanyResponse
    {
        [JsonPropertyName("result")]
        public int Result { get; init; }

        [JsonPropertyName("value")]
        public SgaCompanyValue? Value { get; init; }
    }

    internal sealed class SgaCompanyValue
    {
        [JsonPropertyName("Rows")]
        public IReadOnlyCollection<SgaCompanyRow> Rows { get; init; }
            = [];
    }

    internal sealed class SgaCompanyRow
    {
        [JsonPropertyName("CODIGO")]
        public int Codigo { get; init; }

        [JsonPropertyName("EMPRESA")]
        public string Empresa { get; init; } = string.Empty;

        [JsonPropertyName("DIVISA")]
        public string Divisa { get; init; } = string.Empty;
    }
}