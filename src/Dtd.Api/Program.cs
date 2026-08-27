using Dtd.Api;
using Dtd.Api.HealthChecks;
using Dtd.Api.Modules;
using Dtd.Api.Security;
using Dtd.Application;
using Dtd.Application.Security;
using Dtd.Infrastructure;
using Dtd.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog (console + JSON-friendly for container logs).
builder.Host.UseSerilog((_, loggerConfig) => loggerConfig
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Application & Infrastructure layers.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Contexto de usuario (lee identidad + empresas del token OIDC/Keycloak).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioContexto, HttpUsuarioContexto>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Keycloak:RequireHttpsMetadata", defaultValue: true);

        options.MapInboundClaims = false;

        options.TokenValidationParameters.NameClaimType =
            builder.Configuration["Keycloak:NameClaimType"] ?? "preferred_username";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


// API concerns.
builder.Services.AddProblemDetails(opts =>
{
    opts.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgres", tags: ["ready"]);

// Modules (endpoint groups).
builder.Services.AddDocumentosModule();
builder.Services.AddAlmacenesModule();
builder.Services.AddAgenciasModule();
builder.Services.AddAgenciaBasesModule();
builder.Services.AddCcsModule();
builder.Services.AddDocutenModule();

var app = builder.Build();

// Auto-apply migrations when enabled (handy for local/dev; keep off in production via config).
if (builder.Configuration.GetValue("Database:AutoApplyMigrations", defaultValue: false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DtdDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware order.
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health checks anónimos (k8s probes / readiness no deben requerir token).
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapDocumentosEndpoints();
app.MapAlmacenesEndpoints();
app.MapAgenciasEndpoints();
app.MapAgenciaBasesEndpoints();
app.MapCcsEndpoints();
app.MapDocutenEndpoints();

app.Run();