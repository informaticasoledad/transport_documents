using Dtd.Application.Almacenes;
using Dtd.Application.Documentos.Contracts;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos;
using Dtd.Infrastructure.Configuration;
using Dtd.Infrastructure.Gateways;
using Dtd.Infrastructure.Persistence;
using Dtd.Infrastructure.Persistence.Generators;
using Dtd.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // --- Options (validated on startup) ---
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Descifra Database:Password_Enc (AES-256-GCM, MISMA master key que el client_secret del ERP:
        // ERPAUTH_MASTER_KEY) e inyecta la contraseña en la connection string. La contraseña de
        // PostgreSQL nunca va en claro en el repo. Fail-fast al arrancar si hay bloque pero falta la
        // master key. Si no hay bloque, deja la connection string tal cual.
        services.AddSingleton<IPostConfigureOptions<DatabaseOptions>, DatabaseAuthPostConfigure>();

        services.AddOptions<ErpOptions>()
            .Bind(configuration.GetSection("Erp"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Descifra Erp:ClientSecret_Enc (AES-256-GCM) → ErpOptions.ClientSecret, con la master key
        // inyectada por ERPAUTH_MASTER_KEY / ERPAUTH_MASTER_KEY_FILE. Réplica de la clase del otro
        // programa (mismo ciphertext, mismo montaje de Secret). Fail-fast al arrancar si hay bloque
        // cifrado pero falta/invalida la master key (vía ValidateOnStart). El client_secret es común a
        // todas las empresas; nunca se guarda en la BD ni se loguea.
        services.AddSingleton<IPostConfigureOptions<ErpOptions>, ErpAuthPostConfigure>();

        services.AddOptions<DocutenOptions>()
            .Bind(configuration.GetSection("Docuten"))
            .ValidateDataAnnotations()
            // El API key de Docuten (Docuten:TokenId) es un secret env-only: obligatorio en real
            // (UseMock=false); con el mock no hace falta. Fail-fast al arrancar si falta.
            .Validate(o => o.UseMock || !string.IsNullOrWhiteSpace(o.TokenId),
                "Docuten:TokenId es obligatorio cuando Docuten:UseMock=false. Inyéctalo por env " +
                "(user-secrets en dev / env var 'Docuten__TokenId' desde un Secret de k8s en prod); " +
                "es un secret y nunca va en el repo.")
            .ValidateOnStart();

        // Opciones de MAPEO Docuten visibles para Application (CallbackUrl/DefaultLanguage). Se bindean
        // desde la misma sección "Docuten" y se exponen como singleton plano (sin IOptions en Application,
        // que no puede depender del DocutenOptions de Infrastructure). Sólo lee estas dos claves.
        services.AddOptions<DocutenMappingOptions>()
            .Bind(configuration.GetSection("Docuten"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<DocutenMappingOptions>>().Value);

        services.AddScoped<IDocutenDocumentoProvider, DocutenDocumentoProvider>();

        // Keycloak OIDC: solo se usa cuando Auth:Enabled=true (decisión en Program.cs). Sin
        // [Required] en sus props para que ValidateOnStart no aborte en dev con Auth apagada.
        services.AddOptions<KeycloakOptions>()
            .Bind(configuration.GetSection("Keycloak"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // --- Persistence ---
        services.AddDbContext<DtdDbContext>((sp, options) =>
        {
            var database = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(database.ConnectionString,
                npg => npg.MigrationsAssembly(typeof(DtdDbContext).Assembly.FullName));
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        services.AddScoped<IAgenciaRepository, AgenciaRepository>();
        services.AddScoped<IConductorRepository, ConductorRepository>();
        services.AddScoped<IAgenciaBaseRepository, AgenciaBaseRepository>();
        services.AddScoped<ICcRepository, CcRepository>();
        services.AddScoped<IAlmacenRepository, AlmacenRepository>();
        services.AddScoped<IDocutenCallbackLogRepository, DocutenCallbackLogRepository>();

        services.AddScoped<IDocumentReferenceGenerator,DocumentReferenceGenerator>();

        // --- Integration gateways (real vs mock selected by configuration) ---
        // Per-company ERP endpoint config is resolved at runtime (cached) from `empresas`.
        services.AddMemoryCache();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        var erpOptions = configuration.GetSection("Erp").Get<ErpOptions>() ?? new ErpOptions();
        services.AddScoped<IEmpresaResolver>(sp => new EmpresaResolver(
            sp.GetRequiredService<IEmpresaRepository>(),
            sp.GetRequiredService<IMemoryCache>(),
            TimeSpan.FromMinutes(erpOptions.EndpointCacheMinutes)));

        var erpUseMock = configuration.GetValue("Erp:UseMock", defaultValue: true);
        if (erpUseMock)
        {
            services.AddSingleton<IExpedicionErpGateway, ErpMockGateway>();
        }
        else
        {
            services.AddHttpClient("Erp").AddStandardResilienceHandler();
            // Cliente HTTP para el token endpoint OAuth2 del ERP (client-credentials).
            services.AddHttpClient("ErpToken").AddStandardResilienceHandler();
            services.AddSingleton<IEmpresaTokenProvider, EmpresaTokenProvider>();
            services.AddTransient<IExpedicionErpGateway, ErpGateway>();
        }

        var docutenUseMock = configuration.GetValue("Docuten:UseMock", defaultValue: true);
        if (docutenUseMock)
        {
            services.AddSingleton<IDocutenGateway, DocutenMockGateway>();
        }
        else
        {
            services.AddHttpClient<IDocutenGateway, DocutenGateway>()
                .AddStandardResilienceHandler();
        }

        services.AddScoped<IUsuarioAlmacenesProvider, MockUsuarioAlmacenesProvider>();

        return services;
    }
}
