using Dtd.Application.AgenciaBases;
using Dtd.Application.AgenciaBases.ListarTodasAgenciaBases;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Api.Modules;

/// <summary>
/// Endpoints de gestión del catálogo de agencia-bases (destinatarios) por empresa: crear, actualizar,
/// activar/desactivar y listar (vista de gestión, activos e inactivos). Es el primer catálogo
/// gestionado por API (conductores/almacenes/agencias son seed-only). La autorización de empresa se
/// hace en el handler vía <c>IUsuarioContexto</c>. Los vínculos a almacenes/agencias y los defaults
/// por (almacén, agencia) se gestionan desde <c>AlmacenesModule</c> y <c>AgenciasModule</c>; la
/// asignación al documento, desde <c>DocumentosModule</c>.
/// </summary>
public static class AgenciaBasesModule
{
    public static IServiceCollection AddAgenciaBasesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAgenciaBasesEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var empresas = api.MapGroup("/empresas").WithTags("AgenciaBases");

        // Crea un agenciaBase del catálogo de la empresa y lo vincula a los almacenes/agencias indicados.
        // Defense-in-depth: los ids deben pertenecer a la empresa. 201 con el agenciaBase creado.
        empresas.MapPost("/{empresa}/agencia-bases", async (
            string empresa,
            [FromBody] CrearAgenciaBaseRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CrearAgenciaBaseCommand(
                empresa, req.Codigo, req.Nombre, req.Canal, req.Movil, req.Email,
                req.TaxId, req.Direccion, req.CodigoPostal, req.Municipio, req.CodigoPaisIso,
                req.Language);
            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(dto => Results.Created($"/api/empresas/{empresa}/agencia-bases/{dto.Id}", dto));
        });

        // Actualiza un agenciaBase (Codigo/Empresa inmutables). Replace de vínculos; lista vacía desvincula todo.
        empresas.MapPut("/{empresa}/agencia-bases/{id:guid}", async (
            string empresa, Guid id,
            [FromBody] ActualizarAgenciaBaseRequest req,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ActualizarAgenciaBaseCommand(
                empresa, id, req.Nombre, req.TaxId,
                req.Direccion, req.CodigoPostal, req.Municipio, req.CodigoPaisIso,
                req.Canal, req.Movil, req.Email,
                req.Language);
            var result = await mediator.Send(command, ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        // Activa un agenciaBase (wrapper de CambiarEstadoAgenciaBaseCommand con Activo=true).
        empresas.MapPost("/{empresa}/agencia-bases/{id:guid}/activar", async (
            string empresa, Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CambiarEstadoAgenciaBaseCommand(empresa, id, Activo: true), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        // Desactiva un agenciaBase (wrapper de CambiarEstadoAgenciaBaseCommand con Activo=false).
        empresas.MapPost("/{empresa}/agencia-bases/{id:guid}/desactivar", async (
            string empresa, Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CambiarEstadoAgenciaBaseCommand(empresa, id, Activo: false), ct);
            return result.ToHttpResult(dto => Results.Ok(dto));
        });

        // Lista TODOS los agencia-bases de la empresa (vista de gestión: activos e inactivos).
        empresas.MapGet("/{empresa}/agencia-bases", async (string empresa, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListarTodasAgenciaBasesQuery(empresa), ct);
            return result.ToHttpResult(list => Results.Ok(list));
        });

        return app;
    }
}

/// <summary>Body de <c>POST /empresas/{empresa}/agencia-bases</c>. <c>Codigo</c> es obligatorio y único
/// por empresa; el invariante canal-contacto (email→Email; sms/whatsapp→Móvil) lo valida el handler.</summary>
public sealed record CrearAgenciaBaseRequest(
    string Codigo,
    string Nombre,
    string Canal,
    string? Movil,
    string? Email,
    string? TaxId,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso,
    string Language);

/// <summary>Body de <c>PUT /empresas/{empresa}/agencia-bases/{id}</c>. Sin <c>Codigo</c> (inmutable) ni
/// <c>Empresa</c> (va en la ruta). Listas vacías de vínculos = desvincular todo.</summary>
public sealed record ActualizarAgenciaBaseRequest(
    string Nombre,
    string? TaxId,
    string? Direccion,
    string? CodigoPostal,
    string? Municipio,
    string? CodigoPaisIso,
    string Canal,
    string? Movil,
    string? Email,
    string Language);
