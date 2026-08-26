using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class CcAsignadoConfiguration : IEntityTypeConfiguration<CcAsignado>
{
    public void Configure(EntityTypeBuilder<CcAsignado> builder)
    {
        builder.ToTable("documento_ccs");
        builder.HasKey(x => x.Id);

        // El dominio fija el Id (Guid.NewGuid) en la creación para que sea único ya en memoria
        // (RemoverCc por Id funciona sin depender de la BD). Sin esto, EF Core aplica la convención
        // ValueGeneratedOnAdd a las claves Guid: un nuevo CcAsignado con un Guid no-default añadido a la
        // colección de un agregado ya cargado se trata como EXISTENTE → UPDATE sobre fila inexistente →
        // DbUpdateConcurrencyException. ValueGeneratedNever declara la clave como generada por el cliente
        // → EF hace INSERT. Ver ConductorAsignadoConfiguration.
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Snapshot del catálogo: Id del CC (clave de idempotencia) + código (display) + datos del party
        // Docuten. Es email-only (sin Canal/Movil/TaxId): en Docuten el CC siempre va con
        // signing_role="cc" + channel="email".
        builder.Property(x => x.CcCatalogId).IsRequired();
        builder.Property(x => x.CcCodigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();

        // VO de contacto. `Email` es nullable en el snapshot por simetría con el resto de parties, aunque
        // el catálogo lo exige (siempre llega informado). Columna explícita para evitar el clash de `Valor`.
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(v => v.Valor).HasColumnName("email").HasMaxLength(200);
        });

        // Shadow FK al agregado documento (patrón de Expedicion/ConductorAsignado). No
        // se redeclara el HasMany/WithOne: lo declara el config del agregado DocumentoDigitalTransporte.
        builder.Property<Guid>("DocumentoId");

        // El documento puede tener N CCs (opcionales). El índice único (documento_id, cc_catalog_id)
        // refuerza a nivel de BD la idempotencia por CcCatalogId que enforce el agregado en AsignarCc.
        builder.HasIndex("DocumentoId", nameof(CcAsignado.CcCatalogId)).IsUnique();
    }
}
