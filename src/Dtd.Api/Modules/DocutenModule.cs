using Dtd.Application.Documentos.ProcesarCallbackPlataforma;
using Dtd.Application.Documentos.ListarCallbacksPlataforma;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Common;
using MediatR;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dtd.Api.Modules;

/// <summary>
/// Endpoints de integración con Docuten que NO son del flujo de documentos (que viven en
/// <see cref="DocumentosModule"/>). Hoy sólo el **webhook de callback**: Docuten notifica aquí los
/// cambios de estado del lote/shipments cuando se le pasa un <c>callback_url</c> al crear el lote
/// (ver <c>DocutenMappingOptions.CallbackUrl</c> y <c>DocumentoToDocutenMapper</c>).
/// </summary>
public static class DocutenModule
{
    public static IServiceCollection AddDocutenModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapDocutenEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var docuten = api.MapGroup("/docuten").WithTags("Docuten");

        // Webhook de callback de Docuten. Es un endpoint **público** (lo invoca Docuten desde fuera,
        // no un usuario autenticado): por eso AllowAnonymous, igual que los health checks, aunque
        // Auth:Enabled=true y la FallbackPolicy exija usuario en el resto de endpoints.
        //
        // FASE 1 (ahora): captura-only. No conocemos todavía el payload exacto que nos envía Docuten
        // (el contrato /api/v1/lots detalla el GET de sondeo, pero no el body del webhook). Así que
        // registramos método, ruta, query, cabeceras y body crudo en el log (Serilog a consola) y
        // devolvemos 200 para que Docuten nos lo siga enviando. En cuanto llegue uno real y veamos
        // qué lleva (lot_id/shipment_id/status/...), cablearemos el mapeo al estado del documento
        // (reutilizando ActualizarEstadoDocuten como hace SincronizarEstadoDocuten) y la verificación
        // de firma/origen. Hasta entonces, este endpoint es intencionalmente un "cazador" de la
        // primera notificación real.
        docuten.MapPost("/callback", async (
            HttpContext ctx,
            ILoggerFactory loggerFactory,
            IMediator mediator,
            IDocutenCallbackLogRepository callbackLogRepository,
            IUnitOfWork unitOfWork) =>
        {
            var logger = loggerFactory.CreateLogger("Docuten.Callback");
            var req = ctx.Request;
            req.EnableBuffering();

            string body = string.Empty;
            string? lecturaMensaje = null;
            try
            {
                using var reader = new StreamReader(req.Body, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                req.Body.Position = 0;
            }
            catch (OperationCanceledException ex)
            {
                lecturaMensaje = "Body cancelado durante la lectura.";
                logger.LogWarning(ex, "Docuten callback cancelado mientras se leia el body. Se responde OK para evitar reintentos por una captura incompleta.");
            }
            catch (Microsoft.AspNetCore.Http.BadHttpRequestException ex)
            {
                lecturaMensaje = "Body incompleto.";
                logger.LogWarning(ex, "Docuten callback con body incompleto. Se responde OK para evitar reintentos por una captura incompleta.");
            }
            catch (IOException ex)
            {
                lecturaMensaje = "Error de lectura del body.";
                logger.LogWarning(ex, "Docuten callback con error de lectura del body. Se responde OK para evitar reintentos por una captura incompleta.");
            }


            // Pretty-print si es JSON (lo habitual en Docuten); si no, se loguea el texto crudo.
            var bodyForLog = body;
            if (body.Length > 0)
            {
                var trimmed = body.AsSpan().TrimStart();
                if (!trimmed.IsEmpty && (trimmed[0] is '{' or '['))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        bodyForLog = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                    }
                    catch (JsonException ex)
                    {
                        logger.LogDebug(ex, "Docuten callback recibido con body que parecía JSON, pero no se pudo formatear.");
                    }
                }
            }

            var headers = string.Join(" | ", req.Headers
                .Where(h => !string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                .Select(h => $"{h.Key}={h.Value}"));

            logger.LogInformation(
                "Docuten callback recibido: {Method} {Path}{Query} | Headers: {Headers} | Body: {Body}",
                req.Method, req.Path, req.QueryString, headers, bodyForLog);

            if (string.IsNullOrWhiteSpace(body))
            {
                var unreadablePayload = JsonSerializer.Serialize(new
                {
                    rawPayload = body,
                    reason = lecturaMensaje ?? "Body vacio."
                });

                await callbackLogRepository.AddAsync(
                    new DocutenCallbackLogEntry(
                        Guid.NewGuid(),
                        DateTimeOffset.UtcNow,
                        "unreadable",
                        DocumentoId: null,
                        LotId: null,
                        LotReference: null,
                        ShipmentId: null,
                        ShipmentReference: null,
                        Event: null,
                        Estado: null,
                        Procesado: false,
                        Payload: unreadablePayload,
                        Headers: headers,
                        Mensaje: lecturaMensaje ?? "Body vacio."),
                    CancellationToken.None);

                await unitOfWork.SaveChangesAsync(CancellationToken.None);
                return Results.Ok(new { received = true, processed = false });
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                var result = await mediator.Send(
                    new ProcesarCallbackPlataformaCommand(doc.RootElement.Clone(), body, headers),
                    CancellationToken.None);

                if (result.IsError)
                {
                    logger.LogWarning(
                        "Docuten callback no procesado: {Errors}",
                        string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

                    return Results.Ok(new { received = true, processed = false });
                }

                return Results.Ok(new
                {
                    received = true,
                    processed = result.Value.Procesado,
                    type = result.Value.Tipo,
                    documentoId = result.Value.DocumentoId
                });
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Docuten callback recibido con body no JSON.");
                return Results.Ok(new { received = true, processed = false });
            }
        }).AllowAnonymous();

        docuten.MapGet("/callbacks", async (
            Guid? documentoId,
            int? limit,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new ListarCallbacksPlataformaQuery(documentoId, limit ?? 50),
                ct);

            return result.ToHttpResult(callbacks => Results.Ok(callbacks));
        });

        return app;
    }
}
