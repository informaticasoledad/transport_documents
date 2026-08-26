using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class AlmacenAgenciaConfiguration : IEntityTypeConfiguration<AlmacenAgencia>
{
    public void Configure(EntityTypeBuilder<AlmacenAgencia> builder)
    {
        builder.ToTable("almacen_agencias");
        builder.HasKey(x => new { x.AlmacenId, x.AgenciaId });

        builder.HasOne<Almacen>()
            .WithMany()
            .HasForeignKey(x => x.AlmacenId)
            .HasConstraintName("fk_almacen_agencias_almacenes")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Agencia>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaId)
            .HasConstraintName("fk_almacen_agencias_agencias")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AgenciaBase>()
            .WithMany()
            .HasForeignKey(x => x.AgenciaBaseId)
            .HasConstraintName("fk_almacen_agencias_agencia_bases")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Template>()
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .HasConstraintName("fk_almacen_agencias_templates")
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasIndex(x => x.AgenciaId);
        builder.HasIndex(x => x.AgenciaBaseId);
        builder.HasIndex(x => x.TemplateId);
    }
}
