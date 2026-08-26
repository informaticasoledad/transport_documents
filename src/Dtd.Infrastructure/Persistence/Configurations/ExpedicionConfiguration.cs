using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class ExpedicionConfiguration : IEntityTypeConfiguration<Expedicion>
{
    public void Configure(EntityTypeBuilder<Expedicion> builder)
    {
        builder.ToTable("documento_expediciones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ErpId).HasMaxLength(60).IsRequired();
        builder.Property(x => x.DocumentNumber).HasMaxLength(50);
        builder.Property(x => x.ExpeditionCode).HasMaxLength(50);
        builder.Property(x => x.ExpeditionType);
        builder.Property(x => x.Empresa).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Fecha);
        builder.Property(x => x.Cliente).HasMaxLength(50);
        builder.Property(x => x.Bultos);

        builder.OwnsOne(x => x.Destino, d =>
        {
            d.Property(p => p.Pais).HasColumnName("destino_pais").HasMaxLength(50);
            d.Property(p => p.Provincia).HasColumnName("destino_provincia").HasMaxLength(50);
            d.Property(p => p.CodigoPostal).HasColumnName("destino_codigo_postal").HasMaxLength(20);
            d.Property(p => p.Municipio).HasColumnName("destino_municipio").HasMaxLength(50);
            d.Property(p => p.AlmacenDestino).HasColumnName("destino_almacen_destino").HasMaxLength(50);
            // Dirección del destino (expeditionDestination del ERP) → campo address del shipment en Docuten.
            d.Property(p => p.AddressName).HasColumnName("destino_address_name").HasMaxLength(200);
            d.Property(p => p.AddressStreet).HasColumnName("destino_address_street").HasMaxLength(300);
            // Teléfono del destino (ERP addressPhone1): lo usa el envío directo como móvil del agenciaBase.
            d.Property(p => p.AddressPhone1).HasColumnName("destino_address_phone1").HasMaxLength(50);
        });

        // Shadow FK back to the document aggregate.
        builder.Property<Guid>("DocumentoId");

        // Relación por Id (FK) con los maestros locales almacenes/agencias (mismo scope que el documento,
        // persistido aquí para el dedup). RESTRICT: no se puede borrar un almacén/agencia con expediciones.
        builder.HasOne<Almacen>()
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Agencia>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Envío (shipment) al que pertenece la expedición tras ConstruirEnvios. Nullable: documentos
        // preexistentes a la feature pueden no tener envío. Restrict: borrar un envío no arrastra sus
        // expediciones (éstas pertenecen al documento, no al envío).
        builder.HasOne<Envio>()
            .WithMany()
            .HasForeignKey(x => x.EnvioId)
            .OnDelete(DeleteBehavior.Restrict);

        // An expedition can only belong to one document per company/warehouse/carrier: this is what
        // makes "expediciones no incluidas todavía" trackable. El ERP filtra por warehouseId+carrierId,
        // así que un mismo erpId vuelve siempre bajo el mismo par (almacen, agencia) en la práctica.
        builder.HasIndex(x => new { x.Empresa, x.AlmacenId, x.AgenciaId, x.ErpId }).IsUnique();
    }
}
