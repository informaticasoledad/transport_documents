using Dtd.Application.Almacenes;
using Dtd.Application.Behaviors;
using Dtd.Application.Mapping;
using Dtd.Application.Security;
using Dtd.Application.Templates;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dtd.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(
            assembly,
            lifetime: ServiceLifetime.Scoped,
            includeInternalTypes: true);

        // Mapster: register the mapping config into the global settings so .Adapt<T>() works,
        // and expose an IMapper for explicit mapping.
        MappingRegister.Register(TypeAdapterConfig.GlobalSettings);
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddScoped<IDocumentTemplateValuesBuilder, EcmrTemplateValuesBuilder>();
        services.AddScoped<IDocumentTemplateValuesBuilder, DecaTemplateValuesBuilder>();

        services.AddScoped<
            IDocumentTemplateValuesBuilderResolver,
            DocumentTemplateValuesBuilderResolver>();

        services.AddMemoryCache();

        services.AddScoped<IContextoAccesoService, ContextoAccesoService>();
        services.AddScoped<IAccesoAlmacenService, AccesoAlmacenService>();

        return services;
    }
}
