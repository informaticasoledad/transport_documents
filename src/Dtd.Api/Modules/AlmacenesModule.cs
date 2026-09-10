using Dtd.Application.Almacenes.CrearAlmacen;
using Dtd.Application.Almacenes.EstablecerAgenciaBase;
using Dtd.Application.Almacenes.ListarAgenciasPorAlmacen;
using Dtd.Application.Almacenes.ListarAlmacenes;
using Dtd.Application.Almacenes.ListarCcsDefecto;
using Dtd.Application.Almacenes.ObtenerAlmacen;
using Dtd.Application.Ccs;
using Dtd.Application.Ccs.ListarCcsPorAlmacen;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

public sealed record EstablecerAgenciaBasesDefectoRequest(
    IReadOnlyList<Guid> AgenciaBaseIds);

public sealed record EstablecerCcsDefectoRequest(
    IReadOnlyList<Guid> CcIds);

public sealed record EstablecerAgenciaBaseRequest(
    Guid AgenciaBaseId);


public sealed record CrearAlmacenRequest(
    string Codigo,
    string Nombre,
    string Direccion,
    string CodigoPostal,
    string Ciudad,
    string CodigoPaisIso,
    string? Email,
    string? Telefono,
    string TipoFirmaConsignor = "biometric");

public static class AlmacenesModule
{
    public static IServiceCollection AddAlmacenesModule(
        this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAlmacenesEndpoints(
        this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        var empresas = api
            .MapGroup("/empresas")
            .WithTags("Almacenes");

        empresas.MapGet(
            "/{empresa}/almacenes",
            async (
                string empresa,
                string? texto,
                bool? activo,
                int? page,
                int? pageSize,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarAlmacenesQuery(
                        empresa,
                        texto,
                        activo,
                        page ?? 1,
                        pageSize ?? 20),
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        empresas.MapGet(
            "/{empresa}/almacenes/{almacenId:guid}",
            async (
                string empresa,
                Guid almacenId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ObtenerAlmacenQuery(
                        empresa,
                        almacenId),
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        empresas.MapPost(
            "/{empresa}/almacenes",
            async (
                string empresa,
                [FromBody] CrearAlmacenRequest req,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new CrearAlmacenCommand(
                        empresa,
                        req.Codigo,
                        req.Nombre,
                        req.Direccion,
                        req.CodigoPostal,
                        req.Ciudad,
                        req.CodigoPaisIso,
                        req.Email,
                        req.Telefono,
                        req.TipoFirmaConsignor),
                    ct);

                return result.ToHttpResult(
                    dto => Results.Created(
                        $"/api/empresas/{empresa}/almacenes/{dto.Id}",
                        dto));
            });








        // SEPARACION 

        empresas.MapGet(
            "/{empresa}/almacenes/{almacenId:guid}/agencias",
            async (
                string empresa,
                Guid almacenId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarAgenciasPorAlmacenQuery(
                        empresa,
                        almacenId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        empresas.MapGet(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/agencia-base",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ObtenerAgenciaBaseDefectoQuery(
                        empresa,
                        almacenId,
                        agenciaId),
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        empresas.MapPost(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/agencia-base",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                [FromBody] EstablecerAgenciaBaseRequest req,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command =
                    new EstablecerAgenciaBaseCommand(
                        empresa,
                        almacenId,
                        agenciaId,
                        req.AgenciaBaseId);

                var result = await mediator.Send(
                    command,
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        empresas.MapPut(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/agencia-base",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                [FromBody] EstablecerAgenciaBaseRequest req,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command =
                    new EstablecerAgenciaBaseCommand(
                        empresa,
                        almacenId,
                        agenciaId,
                        req.AgenciaBaseId);

                var result = await mediator.Send(
                    command,
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        empresas.MapGet(
            "/{empresa}/almacenes/{almacenId:guid}/ccs",
            async (
                string empresa,
                Guid almacenId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarCcsPorAlmacenQuery(
                        empresa,
                        almacenId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        empresas.MapGet(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/ccs-default",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarCcsDefectoQuery(
                        empresa,
                        almacenId,
                        agenciaId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        empresas.MapPost(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/ccs-default",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                [FromBody] EstablecerCcsDefectoRequest req,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command =
                    new EstablecerCcsDefectoCommand(
                        empresa,
                        almacenId,
                        agenciaId,
                        req.CcIds);

                var result = await mediator.Send(
                    command,
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        return app;
    }
}