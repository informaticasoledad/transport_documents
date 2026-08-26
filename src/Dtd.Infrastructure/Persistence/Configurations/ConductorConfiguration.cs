using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class ConductorConfiguration : IEntityTypeConfiguration<Conductor>
{
    public void Configure(EntityTypeBuilder<Conductor> builder)
    {
        builder.ToTable("conductores");
        builder.HasKey(x => x.Id);

        // El conductor es per-empresa (3 dígitos) y se vincula M:N a agencias de esa empresa vía
        // `conductor_agencias` (join de persistencia, ver ConductorAgenciaConfiguration). Ya no
        // cuelga 1:N de una agencia.
        builder.Property(x => x.Empresa).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxId).HasMaxLength(30);
        builder.Property(x => x.LicensePlate).HasMaxLength(20);
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Activo).IsRequired();

        // VOs de contacto/canal. Comparten tabla con el agregado y todos exponen `Valor`, así que hay
        // que fijar el nombre de columna explícito para evitar el clash (`valor` colisionaría).
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

        // Un código de conductor es único dentro de su empresa (puede servir a varias agencias,
        // pero todas de la misma empresa).
        builder.HasIndex(x => new { x.Empresa, x.Codigo }).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
