using Dtd.Application.Documentos.CcsDocumento;
using Dtd.Application.Documentos.ConductoresDocumento;
using Dtd.Application.Documentos.EnviarDocumentoADocuten;
using Dtd.Application.Documentos.GenerarDocumento;
using Dtd.Application.Documentos.ListarDocumentos;
using Dtd.Application.Documentos.ListarEventosDocumento;
using Dtd.Application.Documentos.ListarExpedicionesDisponibles;
using Dtd.Application.Documentos.ObtenerDocumento;
using Dtd.Application.Documentos.SincronizarEstadoDocuten;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dtd.Api.Modules;

public static class DocumentosModule
{
    public static IServiceCollection AddDocumentosModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapDocumentosEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var documentos = api.MapGroup("/documentos").WithTags("Documentos");

        documentos.MapPost("/generar", async (
            [FromBody] GenerarDocumentoRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new GenerarDocumentoCommand(
                req.Empresa,
                req.AlmacenId,
                req.AgenciaId,
                req.FechaDesde,
                req.FechaHasta);

            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(id => Results.Created($"/api/documentos/{id}", new { id }));
        });

        documentos.MapPost("/{id:guid}/confirmar", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new EnviarDocumentoADocutenCommand(id), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        documentos.MapPost("/{id:guid}/conductores", async (
            Guid id,
            [FromBody] AsignarConductoresRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new AsignarConductoresDocumentoCommand(id, req.ConductoresId), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        documentos.MapDelete("/{id:guid}/conductores/{conductorId:guid}", async (
            Guid id,
            Guid conductorId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoverConductorDocumentoCommand(id, conductorId), ct);
            return result.ToHttpResult(_ => Results.NoContent());
        });

        documentos.MapPost("/{id:guid}/ccs", async (
            Guid id,
            [FromBody] AsignarCcsRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new AsignarCcsDocumentoCommand(id, req.CcsId), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        documentos.MapDelete("/{id:guid}/ccs/{ccId:guid}", async (
            Guid id,
            Guid ccId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoverCcDocumentoCommand(id, ccId), ct);
            return result.ToHttpResult(_ => Results.NoContent());
        });

        documentos.MapGet("/{id:guid}/eventos", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarEventosDocumentoQuery(id), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        documentos.MapPost("/{id:guid}/sincronizar-estado", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SincronizarEstadoDocutenCommand(id), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        documentos.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ObtenerDocumentoQuery(id), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        documentos.MapGet("/", async (
            string? empresa,
            string? almacenCodigo,
            string? agenciaCodigo,
            DateOnly? fechaDesde,
            DateOnly? fechaHasta,
            string? estado,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new ListarDocumentosQuery(
                empresa,
                almacenCodigo,
                agenciaCodigo,
                fechaDesde,
                fechaHasta,
                estado);

            var result = await mediator.Send(query, ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        var expediciones = api.MapGroup("/expediciones").WithTags("Expediciones");
        expediciones.MapGet("/disponibles", async (
            string empresa,
            Guid almacenId,
            Guid agenciaId,
            DateOnly fechaDesde,
            DateOnly fechaHasta,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new ListarExpedicionesDisponiblesQuery(
                empresa,
                almacenId,
                agenciaId,
                fechaDesde,
                fechaHasta);

            var result = await mediator.Send(query, ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        return app;
    }
}

public sealed record GenerarDocumentoRequest(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    DateOnly FechaDesde,
    DateOnly FechaHasta);

public sealed record AsignarConductoresRequest(IReadOnlyList<Guid> ConductoresId);

public sealed record AsignarCcsRequest(IReadOnlyList<Guid> CcsId);

public sealed record AnularDocumentoRequest(string? Motivo);
