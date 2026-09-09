using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class ConductorConfiguration
    : IEntityTypeConfiguration<Conductor>
{
    public void Configure(EntityTypeBuilder<Conductor> builder)
    {
        builder.ToTable("conductores");

        builder.HasKey(x => x.Id);

        // Catálogo global de conductores.
        // La relación con agencias es M:N mediante conductor_agencias.
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

        builder.Property(x => x.Activo)
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

        builder.Ignore(x => x.DomainEvents);
    }
}