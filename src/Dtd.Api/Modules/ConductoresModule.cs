using Dtd.Application.Conductores.CrearConductor;
using Dtd.Application.Conductores.EliminarConductor;
using Dtd.Application.Conductores.ListarConductores;
using Dtd.Application.Conductores.ListarConductoresCatalogo;
using Dtd.Application.Conductores.ListarConductoresDefault;
using Dtd.Application.Conductores.ModificarConductor;
using Dtd.Application.Conductores.ObtenerConductor;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            .MapGroup("/conductores")
            .WithTags("Conductores");

        // Listado/búsqueda paginada.
        conductores.MapGet(
            "/",
            async (
                string? texto,
                bool? activo,
                int? page,
                int? pageSize,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarConductoresCatalogoQuery(
                        texto,
                        activo,
                        page ?? 1,
                        pageSize ?? 20),
                    ct);

                return result.ToHttpResult(
                    response => Results.Ok(response));
            });

        // Obtener por Id.
        conductores.MapGet(
            "/{conductorId:guid}",
            async (
                Guid conductorId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ObtenerConductorQuery(conductorId),
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        // Crear.
        conductores.MapPost(
            "/",
            async (
                [FromBody] CrearConductorRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new CrearConductorCommand(
                    request.Nombre,
                    request.Canal,
                    request.Movil,
                    request.Email,
                    request.TaxId,
                    request.LicensePlate,
                    request.Language);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    conductor => Results.Created(
                        $"/api/conductores/{conductor.Id}",
                        conductor));
            });

        // Modificar.
        conductores.MapPut(
            "/{conductorId:guid}",
            async (
                Guid conductorId,
                [FromBody] ModificarConductorRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new ModificarConductorCommand(
                    conductorId,
                    request.Nombre,
                    request.Canal,
                    request.Movil,
                    request.Email,
                    request.TaxId,
                    request.LicensePlate,
                    request.Language,
                    request.Activo);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });

        // Eliminar.
        conductores.MapDelete(
            "/{conductorId:guid}",
            async (
                Guid conductorId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new EliminarConductorCommand(conductorId),
                    ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
            });

        var agencias = api
            .MapGroup("/agencias")
            .WithTags("Conductores");

        // Conductores activos vinculados a una agencia.
        agencias.MapGet(
            "/{agenciaId:guid}/conductores",
            async (
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarConductoresQuery(agenciaId),
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

public sealed record CrearConductorRequest(
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string? LicensePlate,
    string Language);

public sealed record ModificarConductorRequest(
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string? LicensePlate,
    string Language,
    bool Activo);