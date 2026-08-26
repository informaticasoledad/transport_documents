using Dtd.Application.Ccs;
using Dtd.Application.Ccs.ListarTodosCcs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

public static class CcsModule
{
    public static IServiceCollection AddCcsModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapCcsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var empresas = api.MapGroup("/empresas").WithTags("Ccs");

        empresas.MapPost("/{empresa}/ccs", async (
            string empresa,
            [FromBody] CrearCcRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CrearCcCommand(
                empresa,
                req.Codigo,
                req.Nombre,
                req.Email,
                req.Language,
                req.Vinculos);

            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(dto => Results.Created($"/api/empresas/{empresa}/ccs/{dto.Id}", dto));
        });

        empresas.MapPut("/{empresa}/ccs/{id:guid}", async (
            string empresa,
            Guid id,
            [FromBody] ActualizarCcRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ActualizarCcCommand(
                empresa,
                id,
                req.Nombre,
                req.Email,
                req.Language,
                req.Vinculos);

            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        empresas.MapPost("/{empresa}/ccs/{id:guid}/activar", async (
            string empresa, Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CambiarEstadoCcCommand(empresa, id, Activo: true), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        empresas.MapPost("/{empresa}/ccs/{id:guid}/desactivar", async (
            string empresa, Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CambiarEstadoCcCommand(empresa, id, Activo: false), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        empresas.MapGet("/{empresa}/ccs", async (string empresa, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarTodosCcsQuery(empresa), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        return app;
    }
}

public sealed record CrearCcRequest(
    string Codigo,
    string Nombre,
    string Email,
    string Language,
    IReadOnlyList<CcVinculoAlmacenAgenciaDto> Vinculos);

public sealed record ActualizarCcRequest(
    string Nombre,
    string Email,
    string Language,
    IReadOnlyList<CcVinculoAlmacenAgenciaDto> Vinculos);
