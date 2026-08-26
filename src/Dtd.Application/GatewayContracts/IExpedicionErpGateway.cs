using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Port for the ERP microservice that is the source of expeditions.
/// Implemented in the infrastructure layer with both a real HTTP client and an in-memory mock,
/// selected via <c>ErpOptions:UseMock</c>.
/// </summary>
public interface IExpedicionErpGateway
{
    /// <summary>Returns the expeditions for the company/warehouse (origin) and carrier within the
    /// given date range. <c>almacenCodigo</c> maps to the ERP <c>warehouseId</c> query param and
    /// <c>agenciaCodigo</c> maps to the ERP <c>carrierId</c>.</summary>
    Task<IReadOnlyList<ExpedicionErpDto>> GetExpedicionesAsync(
        string empresa,
        string almacenCodigo,
        string agenciaCodigo,
        RangoFechas rangoFechas,
        CancellationToken cancellationToken = default);
}