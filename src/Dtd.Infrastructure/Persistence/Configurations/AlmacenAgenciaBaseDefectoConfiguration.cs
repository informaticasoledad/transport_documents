using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.AgenciaBases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AlmacenAgenciaBaseDefectoConfiguration
    : IEntityTypeConfiguration<AlmacenAgenciaBaseDefecto>
{
    public void Configure(EntityTypeBuilder<AlmacenAgenciaBaseDefecto> builder)
    {
        builder.ToTable("almacen_agencia_bases_defecto");
        builder.HasKey(x => new { x.AlmacenId, x.AgenciaId, x.AgenciaBaseId });

        builder.HasOne<Almacen>()
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .HasConstraintName("fk_almacen_agencia_bases_defecto_almacenes")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Agencia>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaId)
            .HasConstraintName("fk_almacen_agencia_bases_defecto_agencias")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AgenciaBase>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaBaseId)
            .HasConstraintName("fk_almacen_agencia_bases_defecto_agencia_bases")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AgenciaBaseId);
    }
}
