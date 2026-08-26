using Dtd.Application.Agencias.ListarAgencias;
using Dtd.Application.Conductores.ListarConductores;
using Dtd.Application.AgenciaBases.ListarAgenciaBases;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

/// <summary>
/// Endpoints de selección de agencias (catálogo per-empresa) y de conductores de una agencia, para el
/// front (empresa → agencias → conductores). La autorización de empresa se hace en el handler vía
/// <c>IUsuarioContexto</c>, igual que <c>DocumentosModule</c>/<c>AlmacenesModule</c>.
/// </summary>
public static class AgenciasModule
{
    public static IServiceCollection AddAgenciasModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAgenciasEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var empresas = api.MapGroup("/empresas").WithTags("Agencias");

        // Agencias activas de una empresa (dropdown empresa → agencias).
        empresas.MapGet("/{empresa}/agencias", async (string empresa, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarAgenciasQuery(empresa), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        // Conductores activos del catálogo de una agencia (dropdown agencia → conductores).
        empresas.MapGet("/{empresa}/agencias/{agenciaCodigo}/conductores", async (
            string empresa, string agenciaCodigo, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarConductoresQuery(empresa, agenciaCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        // AgenciaBases activos del catálogo vinculados a una agencia (dropdown agencia → agencia-bases).
        empresas.MapGet("/{empresa}/agencias/{agenciaCodigo}/agencia-bases", async (
            string empresa, string agenciaCodigo, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarAgenciaBasesQuery(empresa, agenciaCodigo), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        return app;
    }
}
