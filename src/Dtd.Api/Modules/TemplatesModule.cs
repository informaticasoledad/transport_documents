using Dtd.Application.Templates.ListarTemplates;
using MediatR;

namespace Dtd.Api.Modules;

public static class TemplatesModule
{
    public static IServiceCollection AddTemplatesModule(
        this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapTemplatesEndpoints(
        this IEndpointRouteBuilder app)
    {
        var templates = app
            .MapGroup("/api/empresas/{empresa}/templates")
            .WithTags("Templates");

        templates.MapGet(
            "/",
            async (
                string empresa,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarTemplatesQuery(empresa),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });

        return app;
    }
}