namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Port for the Docuten gestor documental. Implementado en infraestructura con un cliente HTTP real
/// (contrato <c>/api/v1/lots</c>) y un mock en memoria, seleccionados vía <c>Docuten:UseMock</c>.
/// 1 DDT = 1 lote con N shipments; la creación es **asíncrona** (devuelve el <c>lot_id</c> en estado
/// <c>pending</c> y Docuten procesa en background notificando a <c>callback_url</c>); el estado se
/// sondea hasta tener webhooks.
/// </summary>
public interface IDocutenGateway
{
    /// <summary>Crea un lote en Docuten (async). Devuelve el <c>lot_id</c> y el estado inicial (pending).</summary>
    Task<DocutenLoteEnvioResult> EnviarAsync(DocutenLoteDto lote, CancellationToken cancellationToken = default);

    /// <summary>Sondea el estado de un lote creado previamente (hasta haber webhooks).</summary>
    Task<DocutenLoteEstadoResult> ObtenerEstadoAsync(string lotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela los shipments aún cancelables de un lote (contrato real: <c>POST /api/v1/shipments/{shipmentId}/cancel</c>
    /// con <c>{"reason":...}</c>, por shipment). Obtiene los <c>shipment_id</c> sondeando el lote y cancela
    /// los que estén en un estado cancelable (<c>pending</c>/<c>created</c>/<c>ready_for_pickup</c>/
    /// <c>pending_delivery</c>); los terminales se ignoran. En fallo lanza <see cref="DocutenGatewayException"/>.
    /// Usado por la anulación forzada desde el front cuando el documento ya fue enviado.
    /// </summary>
    Task CancelarAsync(string lotId, string reason, CancellationToken cancellationToken = default);
}