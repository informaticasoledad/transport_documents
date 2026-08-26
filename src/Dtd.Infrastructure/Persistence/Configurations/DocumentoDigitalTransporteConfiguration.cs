using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class DocumentoDigitalTransporteConfiguration
    : IEntityTypeConfiguration<DocumentoDigitalTransporte>
{
    public void Configure(
        EntityTypeBuilder<DocumentoDigitalTransporte> builder)
    {
        builder.ToTable("documentos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Empresa)
            .HasMaxLength(50)
            .IsRequired();


        builder.Property(x => x.Referencia)
            .HasColumnName("referencia")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Referencia)
            .IsUnique();

        builder.Property(x => x.UsuarioGeneracionId)
            .HasMaxLength(100);

        builder.Property(x => x.FechaGeneracion)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TipoAgrupacion)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PlataformaId)
            .HasColumnName("plataforma_id")
            .HasMaxLength(100);

        builder.Property(x => x.PlataformaEstado)
            .HasColumnName("plataforma_estado")
            .HasMaxLength(50);

        builder.OwnsOne(x => x.RangoFechas, r =>
        {
            r.Property(p => p.FechaDesde)
                .HasColumnName("fecha_desde");

            r.Property(p => p.FechaHasta)
                .HasColumnName("fecha_hasta");
        });

        builder.OwnsOne(x => x.Origen, o =>
        {
            o.Property(p => p.WarehouseId)
                .HasColumnName("origen_warehouse_id")
                .HasMaxLength(50);

            o.Property(p => p.AddressName)
                .HasColumnName("origen_address_name")
                .HasMaxLength(200);

            o.Property(p => p.AddressStreet)
                .HasColumnName("origen_address_street")
                .HasMaxLength(300);

            o.Property(p => p.AddressPhone1)
                .HasColumnName("origen_address_phone1")
                .HasMaxLength(50);

            o.Property(p => p.Zipcode)
                .HasColumnName("origen_zipcode")
                .HasMaxLength(20);

            o.Property(p => p.City)
                .HasColumnName("origen_city")
                .HasMaxLength(100);

            o.Property(p => p.ProvinceName)
                .HasColumnName("origen_province_name")
                .HasMaxLength(100);

            o.Property(p => p.CountryName)
                .HasColumnName("origen_country_name")
                .HasMaxLength(100);

            o.Property(p => p.CountryIsoCode)
                .HasColumnName("origen_country_iso_code")
                .HasMaxLength(2);
        });

        builder.HasMany(x => x.Conductores)
            .WithOne()
            .HasForeignKey("DocumentoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(DocumentoDigitalTransporte.Conductores))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Ccs)
            .WithOne()
            .HasForeignKey("DocumentoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(DocumentoDigitalTransporte.Ccs))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Expediciones)
            .WithOne()
            .HasForeignKey("DocumentoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(DocumentoDigitalTransporte.Expediciones))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Envios)
            .WithOne()
            .HasForeignKey("DocumentoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(DocumentoDigitalTransporte.Envios))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<Almacen>()
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Agencia>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.Empresa,
            x.AlmacenId,
            x.AgenciaId
        });
    }
}
