using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class ConductorAsignadoConfiguration : IEntityTypeConfiguration<ConductorAsignado>
{
    public void Configure(EntityTypeBuilder<ConductorAsignado> builder)
    {
        builder.ToTable("documento_conductores");
        builder.HasKey(x => x.Id);

        // El dominio fija el Id (Guid.NewGuid) en la creación para que sea único ya en memoria (RemoverConductor
        // por Id funciona sin depender de la BD). Sin esto, EF Core aplica la convención ValueGeneratedOnAdd a
        // las claves Guid: un nuevo ConductorAsignado con un Guid no-default añadido a la colección de un
        // agregado ya cargado se trata como EXISTENTE → UPDATE sobre fila inexistente → DbUpdateConcurrencyException.
        // ValueGeneratedNever declara la clave como generada por el cliente → EF hace INSERT.
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Snapshot del catálogo: Id del conductor (clave de idempotencia) + código (display) + datos del party Docuten.
        builder.Property(x => x.ConductorCatalogId).IsRequired();
        builder.Property(x => x.ConductorCodigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxId).HasMaxLength(30);
        builder.Property(x => x.LicensePlate).HasMaxLength(20);
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();

        // VOs de contacto/canal (mismo patrón que ConductorConfiguration: `Valor` colisiona → columnas
        // explícitas). Nullable salvo `channel`, que es obligatorio.
        builder.OwnsOne(x => x.Movil, m =>
        {
            m.Property(v => v.Valor).HasColumnName("movil").HasMaxLength(20);
        });
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(v => v.Valor).HasColumnName("email").HasMaxLength(200);
        });
        builder.OwnsOne(x => x.Canal, c =>
        {
            c.Property(v => v.Valor).HasColumnName("channel").HasMaxLength(10).IsRequired();
        });

        // Shadow FK al agregado documento (patrón de Expedicion).
        builder.Property<Guid>("DocumentoId");

        // Un mismo conductor (por Id de catálogo) no se asigna dos veces al mismo documento (la
        // idempotencia por ConductorCatalogId la enforce el agregado; el índice lo garantiza a nivel
        // de BD por si se bypasea el agregado). El código ya no es único (varias agencias de la misma
        // empresa podrían compartirlo, aunque el catálogo lo evita con el único (empresa, codigo)).
        builder.HasIndex("DocumentoId", nameof(ConductorAsignado.ConductorCatalogId)).IsUnique();
    }
}
