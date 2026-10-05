using Dtd.Application.Documentos.CcsDocumento;
using Dtd.Application.Documentos.ConductoresDocumento;
using Dtd.Application.Documentos.DescargarPdfEnvio;
using Dtd.Application.Documentos.EliminarDocumento;
using Dtd.Application.Documentos.EliminarEnvio;
using Dtd.Application.Documentos.EliminarExpedicion;
using Dtd.Application.Documentos.EnviarDocumentoADocuten;
using Dtd.Application.Documentos.GenerarDocumento;
using Dtd.Application.Documentos.ListarDocumentos;
using Dtd.Application.Documentos.ListarEventosDocumento;
using Dtd.Application.Documentos.ListarExpedicionesDisponibles;
using Dtd.Application.Documentos.ModificarMatriculaDocumento;
using Dtd.Application.Documentos.ModificarPrecintoDocumento;
using Dtd.Application.Documentos.ObtenerDocumento;
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
                 req.FechaHasta,
                 req.Precinto);

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

        /*
        documentos.MapPost("/{id:guid}/sincronizar-estado", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SincronizarEstadoDocutenCommand(id), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        })*/;

        documentos.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ObtenerDocumentoQuery(id), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        documentos.MapGet("/", async (
            string empresa,
            Guid? almacenId,
            Guid? agenciaId,
            DateOnly? fechaDesde,
            DateOnly? fechaHasta,
            bool? finalizado,
            string? estado,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new ListarDocumentosQuery(
                empresa,
                almacenId,
                agenciaId,
                fechaDesde,
                fechaHasta,
                estado,
                finalizado);

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

        documentos.MapDelete("/{id:guid}", async (
    Guid id,
    IMediator mediator,
    CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new EliminarDocumentoCommand(id),
                ct);

            return result.ToHttpResult(
                _ => Results.NoContent());
        });

        documentos.MapDelete(
    "/{id:guid}/envios/{envioId:guid}",
    async (
        Guid id,
        Guid envioId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new EliminarEnvioCommand(
                id,
                envioId),
            ct);

        return result.ToHttpResult(
            _ => Results.NoContent());
    });


        documentos.MapDelete(
    "/{id:guid}/envios/{envioId:guid}/expediciones/{expedicionErpId}",
    async (
        Guid id,
        Guid envioId,
        string expedicionErpId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new EliminarExpedicionCommand(
                id,
                envioId,
                expedicionErpId),
            ct);

        return result.ToHttpResult(
            _ => Results.NoContent());
    });


        documentos.MapPut(
            "/{documentoId:guid}/matricula",
            async (
                Guid documentoId,
                ModificarMatriculaDocumentoRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new ModificarMatriculaDocumentoCommand(
                    documentoId,
                    request.Matricula,
                    request.MatriculaRemolque);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
            });

        documentos.MapPut(
            "/{documentoId:guid}/precinto",
            async (
                Guid documentoId,
                ModificarPrecintoDocumentoRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new ModificarPrecintoDocumentoCommand(
                    documentoId,
                    request.Precinto);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
            });


        documentos.MapGet(
    "/{documentoId:guid}/envios/{envioId:guid}/pdf",
    async (
        Guid documentoId,
        Guid envioId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new DescargarPdfEnvioQuery(
                documentoId,
                envioId),
            ct);

        return result.ToHttpResult(
            pdf => Results.File(
                pdf.Content,
                pdf.ContentType,
                pdf.FileName));
    });
        return app;
    }
}

public sealed record GenerarDocumentoRequest(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    string? Precinto);

public sealed record AsignarConductoresRequest(IReadOnlyList<Guid> ConductoresId);

public sealed record AsignarCcsRequest(IReadOnlyList<Guid> CcsId);

public sealed record ModificarMatriculaDocumentoRequest(
    string? Matricula,
    string? MatriculaRemolque);

public sealed record ModificarPrecintoDocumentoRequest(string? Precinto);

