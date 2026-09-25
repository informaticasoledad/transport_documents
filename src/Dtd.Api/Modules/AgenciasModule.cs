using Dtd.Application.Agencias;
using Dtd.Application.Agencias.CrearAgencia;
using Dtd.Application.Agencias.CrearBaseAgencia;
using Dtd.Application.Agencias.DesvincularConductor;
using Dtd.Application.Agencias.EliminarAgencia;
using Dtd.Application.Agencias.EliminarBaseAgencia;
using Dtd.Application.Agencias.ListarAgenciaBases;
using Dtd.Application.Agencias.ListarAgencias;
using Dtd.Application.Agencias.ModificarAgencia;
using Dtd.Application.Agencias.ModificarBaseAgencia;
using Dtd.Application.Agencias.ObtenerAgencia;
using Dtd.Application.Agencias.ObtenerBaseAgencia;
using Dtd.Application.Agencias.VincularConductor;
using Dtd.Application.Conductores.ListarConductores;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

/// <summary>
/// Endpoints de consulta y mantenimiento del catálogo global de agencias
/// y de sus bases asociadas.
/// </summary>
public static class AgenciasModule
{
    public static IServiceCollection AddAgenciasModule(
        this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAgenciasEndpoints(
        this IEndpointRouteBuilder app)
    {
        var agencias = app
            .MapGroup("/api/agencias")
            .WithTags("Agencias");

        // Listado de agencias.
        agencias.MapGet(
          "/",
          async (
              string? texto,
              bool? activa,
              bool? envioDirecto,
              int? page,
              int? pageSize,
              IMediator mediator,
              CancellationToken ct) =>
          {
              var result = await mediator.Send(
                  new ListarAgenciasQuery(
                      texto,
                      activa,
                      envioDirecto,
                      page ?? 1,
                      pageSize ?? 20),
                  ct);

              return result.ToHttpResult(
                  response => Results.Ok(response));
          });

        // Obtener una agencia concreta.
        agencias.MapGet(
            "/{agenciaId:guid}",
            async (
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ObtenerAgenciaQuery(agenciaId),
                    ct);

                return result.ToHttpResult(
                    agencia => Results.Ok(agencia));
            });

        // Crear agencia.
        agencias.MapPost(
            "/",
            async (
                CrearAgenciaRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new CrearAgenciaCommand(
                    request.Codigo,
                    request.Nombre,
                    request.AgenciaQs,
                    request.EnvioDirecto);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    agencia => Results.Created(
                        $"/api/agencias/{agencia.Id}",
                        agencia));
            });

        // Modificar agencia.
        agencias.MapPut(
            "/{agenciaId:guid}",
            async (
                Guid agenciaId,
                ModificarAgenciaRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new ModificarAgenciaCommand(
                    agenciaId,
                    request.Codigo,
                    request.Nombre,
                    request.AgenciaQs,
                    request.Activa,
                    request.EnvioDirecto);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    agencia => Results.Ok(agencia));
            });

        // Baja lógica de agencia.
        agencias.MapDelete(
            "/{agenciaId:guid}",
            async (
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new EliminarAgenciaCommand(agenciaId),
                    ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
            });

        // Bases vinculadas a una agencia.
        agencias.MapGet(
            "/{agenciaId:guid}/bases",
            async (
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ListarAgenciaBasesQuery(agenciaId),
                    ct);

                return result.ToHttpResult(
                    list => Results.Ok(list));
            });


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

        // Crear base de agencia.
        agencias.MapPost(
            "/{agenciaId:guid}/bases",
            async (
                Guid agenciaId,
                CrearBaseAgenciaRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new CrearBaseAgenciaCommand(
                    agenciaId,
                    request.Codigo,
                    request.Nombre,
                    request.Canal,
                    request.Movil,
                    request.Email,
                    request.TaxId,
                    request.Language,
                    request.Direccion,
                    request.CodigoPostal,
                    request.Municipio,
                    request.CodigoPaisIso);

                var result = await mediator.Send(
                    command,
                    ct);

                return result.ToHttpResult(
                    baseAgencia => Results.Created(
                        $"/api/agencias/{agenciaId}/bases/{baseAgencia.Id}",
                        baseAgencia));
            });

        agencias.MapPut(
            "/{agenciaId:guid}/bases/{baseId:guid}",
            async (
                Guid agenciaId,
                Guid baseId,
                ModificarBaseAgenciaRequest request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new ModificarBaseAgenciaCommand(
                    agenciaId,
                    baseId,
                    request.Nombre,
                    request.Canal,
                    request.Movil,
                    request.Email,
                    request.TaxId,
                    request.Language,
                    request.Direccion,
                    request.CodigoPostal,
                    request.Municipio,
                    request.CodigoPaisIso);

                var result = await mediator.Send(command, ct);

                return result.ToHttpResult(
                    baseAgencia => Results.Ok(baseAgencia));
            });

        // Eliminar físicamente una base de agencia.
        agencias.MapDelete(
            "/{agenciaId:guid}/bases/{baseId:guid}",
            async (
                Guid agenciaId,
                Guid baseId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new EliminarBaseAgenciaCommand(
                        agenciaId,
                        baseId),
                    ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
            });


        // Obtener una base concreta de una agencia.
        agencias.MapGet(
            "/{agenciaId:guid}/bases/{baseId:guid}",
            async (
                Guid agenciaId,
                Guid baseId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new ObtenerBaseAgenciaQuery(
                        agenciaId,
                        baseId),
                    ct);

                return result.ToHttpResult(
                    baseAgencia => Results.Ok(baseAgencia));
            });

        // Vincular conductor a agencia 
        agencias.MapPost(
            "/{agenciaId:guid}/conductores/{conductorId:guid}",
            async (
                Guid agenciaId,
                Guid conductorId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new VincularConductorAgenciaCommand(
                        agenciaId,
                        conductorId),
                    ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
            });

        agencias.MapDelete(
    "/{agenciaId:guid}/conductores/{conductorId:guid}",
    async (
        Guid agenciaId,
        Guid conductorId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new DesvincularConductorAgenciaCommand(
                agenciaId,
                conductorId),
            ct);

        return result.ToHttpResult(
            _ => Results.NoContent());
    });
        return app;
    }
}



public sealed record CrearAgenciaRequest(
    string Codigo,
    string Nombre,
    string? AgenciaQs,
    bool EnvioDirecto);

public sealed record ModificarAgenciaRequest(
    string Codigo,
    string Nombre,
    string? AgenciaQs,
    bool Activa,
    bool EnvioDirecto);

public sealed record CrearBaseAgenciaRequest(
    string Codigo,
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string Language,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso);

public sealed record ModificarBaseAgenciaRequest(
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string Language,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso);