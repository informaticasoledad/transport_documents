using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class ConductorAsignadoConfiguration
    : IEntityTypeConfiguration<ConductorAsignado>
{
    public void Configure(EntityTypeBuilder<ConductorAsignado> builder)
    {
        builder.ToTable("documento_conductores");

        builder.HasKey(x => x.Id);

        // El dominio fija el Id (Guid.NewGuid) en la creación para que sea único ya en memoria.
        // ValueGeneratedNever indica a EF Core que la clave la genera el cliente y debe hacer INSERT.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Snapshot del catálogo:
        // Id del conductor como clave de idempotencia + datos utilizados por Docuten.
        builder.Property(x => x.ConductorCatalogId)
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TaxId)
            .HasMaxLength(30);

        builder.Property(x => x.LicensePlate)
            .HasMaxLength(20);

        builder.Property(x => x.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.OwnsOne(x => x.Movil, m =>
        {
            m.Property(v => v.Valor)
                .HasColumnName("movil")
                .HasMaxLength(20);
        });

        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(v => v.Valor)
                .HasColumnName("email")
                .HasMaxLength(200);
        });

        builder.OwnsOne(x => x.Canal, c =>
        {
            c.Property(v => v.Valor)
                .HasColumnName("channel")
                .HasMaxLength(10)
                .IsRequired();
        });

        // Shadow FK al agregado documento.
        builder.Property<Guid>("DocumentoId");

        // Un mismo conductor del catálogo no puede asignarse dos veces
        // al mismo documento.
        builder.HasIndex(
            "DocumentoId",
            nameof(ConductorAsignado.ConductorCatalogId))
            .IsUnique();
    }
}