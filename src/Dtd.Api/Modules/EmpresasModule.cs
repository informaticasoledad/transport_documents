using Dtd.Application.Empresas.ListarEmpresasPermitidas;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

public static class EmpresasModule
{
    public static IServiceCollection AddEmpresasModule(
        this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapEmpresasEndpoints(
        this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        var empresas = api
            .MapGroup("/empresas")
            .WithTags("Empresas");

        empresas.MapGet(
            "/permitidas",
            async (
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(
                    new ListarEmpresasPermitidasQuery(),
                    cancellationToken);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        return app;
    }
}