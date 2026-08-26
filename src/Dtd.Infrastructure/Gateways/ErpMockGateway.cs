using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Infrastructure.Gateways;

/// <summary>
/// In-memory ERP gateway used while the real HTTP contract is being finalised.
/// Returns the new expedition DTO shape (origin common to all expeditions of the agency,
/// destination per expedition, line details so Bultos = Count varies, both expedition types).
/// </summary>
internal sealed class ErpMockGateway : IExpedicionErpGateway
{
    private const string OriginName = "DELEGACION MIRANDA";

    public Task<IReadOnlyList<ExpedicionErpDto>> GetExpedicionesAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        RangoFechas rangoFechas,
        CancellationToken cancellationToken = default)
    {
        var expediciones = new List<ExpedicionErpDto>();
        var fecha = rangoFechas.FechaDesde;

        // Up to 5 expeditions across the range, alternating type 1 (customer delivery) and
        // type 2 (warehouse transfer, no customerId, with destinationWarehouseId).
        var count = 0;
        for (; fecha <= rangoFechas.FechaHasta && count < 5; fecha = fecha.AddDays(1), count++)
        {
            var isTransfer = count % 2 == 1;
            var detailCount = (count % 3) + 1; // 1..3 líneas → Bultos varía

            expediciones.Add(new ExpedicionErpDto
            {
                Id = $"{empresa}-{almacenCodigo}-{agenciaCodigo}-{fecha:yyyyMMdd}-{count + 1:D3}",
                // empresa no viaja en el body del ERP; el gateway la estampa. El almacén/agencia se
                // persisten por Id (FK) desde el documento; el gateway ya no estampa el código de agencia.
                Empresa = empresa,
                DocumentNumber = $"2028140{100 + count:D3}",
                ExpeditionDate = fecha.ToDateTime(TimeOnly.MinValue),
                ExpeditionCode = $"11650{700 + count:D3}",
                ExpeditionType = isTransfer ? 2 : 1,
                OriginWarehouseId = almacenCodigo,
                CustomerId = isTransfer ? null : (1000 + count).ToString(),
                DestinationWarehouseId = isTransfer ? "79" : null,
                ExpeditionOrigin = BuildOrigin(almacenCodigo),
                ExpeditionDestination = BuildDestination(count, isTransfer),
                ExpeditionDetails = Enumerable.Range(0, detailCount)
                    .Select(i => new ExpeditionDetailErpDto
                    {
                        ProductId = $"0101{i:D12}",
                        ProductName = $"Neumático de prueba {count}-{i}",
                        ProductUnits = 2m
                    })
                    .ToList()
            });
        }

        return Task.FromResult<IReadOnlyList<ExpedicionErpDto>>(expediciones);
    }

    private static ExpeditionOriginErpDto BuildOrigin(string warehouseId) => new()
    {
        Id = warehouseId,
        AddressName = OriginName,
        AddressStreet = "RIBERAS DEL EBRO N41 P.I.",
        AddressPhone1 = "",
        Zipcode = "09200",
        City = "MIRANDA DE EBRO",
        ProvinceName = "BURGOS",
        CountryName = "ESPAÑA",
        CountryIsoCode = "ES"
    };

    private static ExpeditionDestinationErpDto BuildDestination(int count, bool isTransfer) => new()
    {
        Id = isTransfer ? "79" : (1000 + count).ToString(),
        AddressName = isTransfer ? "VALLADOLID TALLER" : $"CLIENTE {1000 + count}",
        AddressStreet = $"C/ DESTINO {count}",
        AddressPhone1 = "920000000",
        Zipcode = (10000 + count).ToString(),
        City = isTransfer ? "Valladolid" : "Destino",
        ProvinceName = isTransfer ? "VALLADOLID" : "PROVINCIA",
        CountryName = "ESPAÑA",
        CountryIsoCode = "ES"
    };
}