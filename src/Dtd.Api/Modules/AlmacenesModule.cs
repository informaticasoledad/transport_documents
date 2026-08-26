using Dtd.Application.Almacenes.EstablecerAgenciaBase;
using Dtd.Application.Almacenes.ListarAgenciasPorAlmacen;
using Dtd.Application.Almacenes.ListarAlmacenes;
using Dtd.Application.Almacenes.ListarCcsDefecto;
using Dtd.Application.Almacenes.ListarConductoresDefecto;
using Dtd.Application.Almacenes.ListarAgenciaBasesDefecto;
using Dtd.Application.Ccs;
using Dtd.Application.Ccs.ListarCcsPorAlmacen;
using Dtd.Application.AgenciaBases;
using Dtd.Application.AgenciaBases.ListarAgenciaBasesPorAlmacen;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

public sealed record EstablecerAgenciaBasesDefectoRequest(IReadOnlyList<Guid> AgenciaBaseIds);

public sealed record EstablecerCcsDefectoRequest(IReadOnlyList<Guid> CcIds);

public sealed record EstablecerAgenciaBaseRequest(Guid AgenciaBaseId);

public static class AlmacenesModule
{
    public static IServiceCollection AddAlmacenesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAlmacenesEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var empresas = api.MapGroup("/empresas").WithTags("Almacenes");

        empresas.MapGet("/{empresa}/almacenes", async (
            string empresa,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarAlmacenesQuery(empresa), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapGet("/{empresa}/almacenes/{almacenCodigo}/agencias", async (
            string empresa,
            string almacenCodigo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarAgenciasPorAlmacenQuery(empresa, almacenCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapGet("/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/conductores-default", async (
            string empresa,
            string almacenCodigo,
            string agenciaCodigo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarConductoresDefectoQuery(empresa, almacenCodigo, agenciaCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapGet("/{empresa}/almacenes/{almacenCodigo}/agencia-bases", async (
            string empresa,
            string almacenCodigo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarAgenciaBasesPorAlmacenQuery(empresa, almacenCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapGet("/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/agencia-bases-default", async (
            string empresa,
            string almacenCodigo,
            string agenciaCodigo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarAgenciaBasesDefectoQuery(empresa, almacenCodigo, agenciaCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapPost("/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/agencia-bases-default", async (
            string empresa,
            string almacenCodigo,
            string agenciaCodigo,
            [FromBody] EstablecerAgenciaBasesDefectoRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new EstablecerAgenciaBasesDefectoCommand(
                empresa,
                almacenCodigo,
                agenciaCodigo,
                req.AgenciaBaseIds);

            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapPut("/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/agencia-base", async (
            string empresa,
            string almacenCodigo,
            string agenciaCodigo,
            [FromBody] EstablecerAgenciaBaseRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new EstablecerAgenciaBaseCommand(
                empresa,
                almacenCodigo,
                agenciaCodigo,
                req.AgenciaBaseId);

            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        empresas.MapGet("/{empresa}/almacenes/{almacenCodigo}/ccs", async (
            string empresa,
            string almacenCodigo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarCcsPorAlmacenQuery(empresa, almacenCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapGet("/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/ccs-default", async (
            string empresa,
            string almacenCodigo,
            string agenciaCodigo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarCcsDefectoQuery(empresa, almacenCodigo, agenciaCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        empresas.MapPost("/{empresa}/almacenes/{almacenCodigo}/agencias/{agenciaCodigo}/ccs-default", async (
            string empresa,
            string almacenCodigo,
            string agenciaCodigo,
            [FromBody] EstablecerCcsDefectoRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new EstablecerCcsDefectoCommand(
                empresa,
                almacenCodigo,
                agenciaCodigo,
                req.CcIds);

            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        return app;
    }
}
