using Dtd.Application.Almacenes.CrearAlmacen;
using Dtd.Application.Almacenes.EliminarAlmacen;
using Dtd.Application.Almacenes.EliminarAlmacenAgencia;
using Dtd.Application.Almacenes.EliminarCcDefecto;
using Dtd.Application.Almacenes.EstablecerAgenciaBase;
using Dtd.Application.Almacenes.ListarAgenciasPorAlmacen;
using Dtd.Application.Almacenes.ListarAlmacenes;
using Dtd.Application.Almacenes.ListarCcsDefecto;
using Dtd.Application.Almacenes.ModificarAlmacen;
using Dtd.Application.Almacenes.ModificarAlmacenAgencia;
using Dtd.Application.Almacenes.ObtenerAlmacen;
using Dtd.Application.Almacenes.VincularAlmacenAgencia;
using Dtd.Application.Almacenes.VincularAlmacenAgenciaCc;
using Dtd.Application.Ccs.AgregarCcDefecto;
using Dtd.Application.Ccs.ListarCcsPorAlmacen;
using Dtd.Application.Conductores.ListarConductoresDefault;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

public sealed record ModificarAlmacenRequest(
    string Nombre,
    string Direccion,
    string CodigoPostal,
    string Ciudad,
    string CodigoPaisIso,
    string? Email,
    string? Telefono,
    string TipoFirmaConsignor,
    bool Activo);

public sealed record VincularAlmacenAgenciaCcRequest(
    Guid CcId,
    bool PorDefecto);

public sealed record VincularAlmacenAgenciaRequest(
    Guid AgenciaId,
    Guid TemplateId,
    Guid? AgenciaBaseId);

public sealed record ModificarAlmacenAgenciaRequest(
    Guid TemplateId,
    Guid? AgenciaBaseId);

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


        empresas.MapPut(
    "/{empresa}/almacenes/{almacenId:guid}",
    async (
        string empresa,
        Guid almacenId,
        [FromBody] ModificarAlmacenRequest req,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new ModificarAlmacenCommand(
                empresa,
                almacenId,
                req.Nombre,
                req.Direccion,
                req.CodigoPostal,
                req.Ciudad,
                req.CodigoPaisIso,
                req.Email,
                req.Telefono,
                req.TipoFirmaConsignor,
                req.Activo),
            ct);

        return result.ToHttpResult(
            dto => Results.Ok(dto));
    });


    empresas.MapDelete(
    "/{empresa}/almacenes/{almacenId:guid}",
    async (
        string empresa,
        Guid almacenId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new EliminarAlmacenCommand(
                empresa,
                almacenId),
            ct);

        return result.ToHttpResult(
            _ => Results.NoContent());
    });



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

        empresas.MapPost(
    "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/ccs",
    async (
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        [FromBody] VincularAlmacenAgenciaCcRequest req,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new VincularAlmacenAgenciaCcCommand(
                empresa,
                almacenId,
                agenciaId,
                req.CcId,
                req.PorDefecto),
            ct);

        return result.ToHttpResult(
            dto => Results.Ok(dto));
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
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/ccs-default/{ccId:guid}",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                Guid ccId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new AgregarCcDefectoCommand(
                        empresa,
                        almacenId,
                        agenciaId,
                        ccId),
                    ct);

                return result.ToHttpResult(
                    dto => Results.Ok(dto));
            });


        empresas.MapDelete(
    "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}/ccs-default/{ccId:guid}",
    async (
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        Guid ccId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new EliminarCcDefectoCommand(
                empresa,
                almacenId,
                agenciaId,
                ccId),
            ct);

        return result.ToHttpResult(
            _ => Results.NoContent());
    });


        empresas.MapPost(
    "/{empresa}/almacenes/{almacenId:guid}/agencias",
    async (
        string empresa,
        Guid almacenId,
        [FromBody] VincularAlmacenAgenciaRequest req,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new VincularAlmacenAgenciaCommand(
                empresa,
                almacenId,
                req.AgenciaId,
                req.TemplateId,
                req.AgenciaBaseId),
            ct);

        return result.ToHttpResult(
            dto => Results.Ok(dto));
    });

        empresas.MapPut(
    "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}",
    async (
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        [FromBody] ModificarAlmacenAgenciaRequest req,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(
            new ModificarAlmacenAgenciaCommand(
                empresa,
                almacenId,
                agenciaId,
                req.TemplateId,
                req.AgenciaBaseId),
            ct);

        return result.ToHttpResult(
            dto => Results.Ok(dto));
    });


        empresas.MapDelete(
            "/{empresa}/almacenes/{almacenId:guid}/agencias/{agenciaId:guid}",
            async (
                string empresa,
                Guid almacenId,
                Guid agenciaId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new EliminarAlmacenAgenciaCommand(
                        empresa,
                        almacenId,
                        agenciaId),
                    ct);

                return result.ToHttpResult(
                    _ => Results.NoContent());
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