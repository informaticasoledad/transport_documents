using Dtd.Application.Conductores.ListarConductores;
using Dtd.Application.Conductores.ListarConductoresDefault;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

public static class ConductoresModule
{
    public static IServiceCollection AddConductoresModule(
        this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapConductoresEndpoints(
        this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        var conductores = api
            .MapGroup("/agencias")
            .WithTags("Conductores");

        // Todos los conductores activos de una agencia.
        conductores.MapGet(
            "/{agenciaId:guid}/conductores",
            async (
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarConductoresQuery(
                        agenciaId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        var empresas = api
            .MapGroup("/empresas")
            .WithTags("Conductores");

        // Conductores por defecto de almacén + agencia.
        empresas.MapGet(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/conductores-default",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarConductoresDefaultQuery(
                        empresa,
                        almacenId,
                        agenciaId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        return app;
    }
}