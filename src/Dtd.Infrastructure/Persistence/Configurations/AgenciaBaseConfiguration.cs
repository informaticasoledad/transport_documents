using Dtd.Domain.Agencias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AgenciaBaseConfiguration
    : IEntityTypeConfiguration<AgenciaBase>
{
    public void Configure(EntityTypeBuilder<AgenciaBase> builder)
    {
        builder.ToTable("agencia_bases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AgenciaId)
            .IsRequired();

        builder.Property(x => x.Codigo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TaxId)
            .HasMaxLength(30);

        builder.Property(x => x.Direccion)
            .HasMaxLength(300);

        builder.Property(x => x.CodigoPostal)
            .HasMaxLength(20);

        builder.Property(x => x.Municipio)
            .HasMaxLength(100);

        builder.Property(x => x.CodigoPaisIso)
            .HasMaxLength(2);

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

        builder.HasIndex(x => new
        {
            x.AgenciaId,
            x.Codigo
        })
        .IsUnique();
    }
}