using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AlmacenAgenciaConductorDefectoConfiguration
    : IEntityTypeConfiguration<AlmacenAgenciaConductorDefecto>
{
    public void Configure(EntityTypeBuilder<AlmacenAgenciaConductorDefecto> builder)
    {
        builder.ToTable("almacen_agencia_conductores_defecto");
        builder.HasKey(x => new { x.AlmacenId, x.AgenciaId, x.ConductorId });

        builder.HasOne<Almacen>()
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Agencia>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Conductor>()
            .WithMany()
            .HasForeignKey(x => x.ConductorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice sobre conductor_id para el delete en cascada.
        builder.HasIndex(x => x.ConductorId);
    }
}