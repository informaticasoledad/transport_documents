using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dtd.Application.Behaviors;
using Dtd.Application.Mapping;

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

        return services;
    }
}
