using Dtd.Domain.Agencias;
using Dtd.Domain.Conductores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class ConductorAgenciaConfiguration : IEntityTypeConfiguration<ConductorAgencia>
{
    public void Configure(EntityTypeBuilder<ConductorAgencia> builder)
    {
        builder.ToTable("conductor_agencias");
        builder.HasKey(x => new { x.ConductorId, x.AgenciaId });

        builder.HasOne<Conductor>()
            .WithMany()
            .HasForeignKey(x => x.ConductorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Agencia>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice sobre la FK a agencias (la otra mitad de la PK ya indexa conductor_id).
        builder.HasIndex(x => x.AgenciaId);
    }
}