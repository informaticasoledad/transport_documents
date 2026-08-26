using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Domain.Empresas;
using Dtd.Domain.Templates;
using Dtd.Infrastructure.Persistence.IntegrationLogs;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the digital transport documents PostgreSQL database.
/// Uses snake_case naming (see <see cref="DependencyInjection"/>) and applies all
/// <see cref="IEntityTypeConfiguration{T}"/> classes from this assembly.
/// </summary>
public sealed class DtdDbContext : DbContext
{
    public DtdDbContext(DbContextOptions<DtdDbContext> options) : base(options) { }

    public DbSet<DocumentoDigitalTransporte> Documentos => Set<DocumentoDigitalTransporte>();
    public DbSet<Expedicion> Expediciones => Set<Expedicion>();
    public DbSet<Agencia> Agencias => Set<Agencia>();
    public DbSet<Conductor> Conductores => Set<Conductor>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Almacen> Almacenes => Set<Almacen>();
    public DbSet<AlmacenAgencia> AlmacenAgencias => Set<AlmacenAgencia>();
    public DbSet<AlmacenAgenciaConductorDefecto> AlmacenAgenciaConductoresDefecto => Set<AlmacenAgenciaConductorDefecto>();
    public DbSet<ConductorAgencia> ConductorAgencias => Set<ConductorAgencia>();
    public DbSet<AgenciaBase> AgenciaBases => Set<AgenciaBase>();
    public DbSet<AlmacenAgenciaBaseDefecto> AlmacenAgenciaBasesDefecto => Set<AlmacenAgenciaBaseDefecto>();
    public DbSet<Cc> Ccs => Set<Cc>();
    public DbSet<AlmacenAgenciaCc> AlmacenAgenciaCcs => Set<AlmacenAgenciaCc>();
    public DbSet<Template> Templates => Set<Template>();
    internal DbSet<DocutenCallbackLog> DocutenCallbackLogs => Set<DocutenCallbackLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DtdDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
