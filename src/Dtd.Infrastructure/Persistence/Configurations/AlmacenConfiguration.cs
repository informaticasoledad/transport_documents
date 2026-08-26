using Dtd.Domain.Almacenes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AlmacenConfiguration : IEntityTypeConfiguration<Almacen>
{
    public void Configure(EntityTypeBuilder<Almacen> builder)
    {
        builder.ToTable("almacenes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Empresa).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Direccion).HasMaxLength(300);
        builder.Property(x => x.CodigoPostal).HasMaxLength(20);
        builder.Property(x => x.Ciudad).HasMaxLength(200);
        builder.Property(x => x.CodigoPaisIso).HasMaxLength(10);
        builder.Property(x => x.Telefono).HasMaxLength(30);
        builder.Property(x => x.TipoFirmaConsignor)
            .HasColumnName("tipo_firma_consignor")
            .HasMaxLength(20)
            .HasDefaultValue("biometric")
            .IsRequired();
        builder.Property(x => x.Activo);

        // Email del almacén (canal del consignor). Opcional, propiedad del VO Email.
        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(p => p.Valor).HasColumnName("email").HasMaxLength(200);
        });

        // Clave natural per-empresa: (empresa, codigo).
        builder.HasIndex(x => new { x.Empresa, x.Codigo }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
