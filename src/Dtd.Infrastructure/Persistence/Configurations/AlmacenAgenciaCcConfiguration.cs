using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AlmacenAgenciaCcConfiguration
    : IEntityTypeConfiguration<AlmacenAgenciaCc>
{
    public void Configure(EntityTypeBuilder<AlmacenAgenciaCc> builder)
    {
        builder.ToTable("almacen_agencia_ccs");
        builder.HasKey(x => new { x.AlmacenId, x.AgenciaId, x.CcId });

        builder.Property(x => x.PorDefecto)
            .HasColumnName("por_defecto")
            .IsRequired();

        builder.HasOne<AlmacenAgencia>()
            .WithMany()
            .HasForeignKey(x => new { x.AlmacenId, x.AgenciaId })
            .HasConstraintName("fk_almacen_agencia_ccs_almacen_agencias")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Cc>()
            .WithMany()
            .HasForeignKey(x => x.CcId)
            .HasConstraintName("fk_almacen_agencia_ccs_ccs")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CcId);
    }
}
