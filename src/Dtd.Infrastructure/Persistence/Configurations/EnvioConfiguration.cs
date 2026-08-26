using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class EnvioConfiguration : IEntityTypeConfiguration<Envio>
{
    public void Configure(EntityTypeBuilder<Envio> builder)
    {
        builder.ToTable("documento_envios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Orden)
            .IsRequired();

        builder.Property(x => x.Referencia)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Bultos)
            .IsRequired();

        builder.Property(x => x.PlataformaEnvioId)
            .HasColumnName("plataforma_envio_id")
            .HasMaxLength(100);

        builder.Property(x => x.PlataformaEnvioEstado)
            .HasColumnName("plataforma_envio_estado")
            .HasMaxLength(50);

        builder.OwnsOne(x => x.Destino, d =>
        {
            d.Property(p => p.Codigo)
                .HasColumnName("destino_codigo")
                .HasMaxLength(50);

            d.Property(p => p.Nombre)
                .HasColumnName("destino_nombre")
                .HasMaxLength(200);

            d.Property(p => p.Direccion)
                .HasColumnName("destino_direccion")
                .HasMaxLength(300);

            d.Property(p => p.CodigoPostal)
                .HasColumnName("destino_codigo_postal")
                .HasMaxLength(20);

            d.Property(p => p.Ciudad)
                .HasColumnName("destino_ciudad")
                .HasMaxLength(100);

            d.Property(p => p.CodigoPais)
                .HasColumnName("destino_codigo_pais")
                .HasMaxLength(2);

            d.Property(p => p.Telefono)
                .HasColumnName("destino_telefono")
                .HasMaxLength(50);
        });

        builder.Property<Guid>("DocumentoId");
    }
}
