using Dtd.Domain.Agencias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AgenciaConfiguration : IEntityTypeConfiguration<Agencia>
{
    public void Configure(EntityTypeBuilder<Agencia> builder)
    {
        builder.ToTable("agencias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Codigo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IdentificadorFiscal)
            .HasColumnName("identificador_fiscal")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RequiereDatosPorteador)
            .HasColumnName("requiere_datos_porteador")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.Activa)
            .IsRequired();

        builder.Property(x => x.AgenciaQs)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.EntregaEnDestino)
            .IsRequired();

        builder.Property(x => x.RequierePrecinto)
            .IsRequired();

        // Datos de dirección.
        builder.Property(x => x.Direccion)
            .HasMaxLength(250);

        builder.Property(x => x.CodigoPostal)
            .HasMaxLength(20);

        builder.Property(x => x.Municipio)
            .HasMaxLength(100);

        builder.Property(x => x.CodigoPaisIso)
            .HasMaxLength(2);

        // El código de agencia es global.
        builder.HasIndex(x => x.Codigo)
            .IsUnique();

        builder.HasMany(x => x.Bases)
            .WithOne()
            .HasForeignKey(x => x.AgenciaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.Ignore(x => x.DomainEvents);
    }
}