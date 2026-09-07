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
        var empresas = app
            .MapGroup("/api/empresas")
            .WithTags("Conductores");

        // Todos los conductores activos de una agencia.
        empresas.MapGet(
            "/{empresa}/agencias/{agenciaId:guid}/conductores",
            async (
                string empresa,
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarConductoresQuery(
                        empresa,
                        agenciaId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

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