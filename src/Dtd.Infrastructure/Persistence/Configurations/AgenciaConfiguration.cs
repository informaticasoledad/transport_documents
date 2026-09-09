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

        builder.Property(x => x.Codigo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Activa)
            .IsRequired();

        builder.Property(x => x.AgenciaQs)
            .HasMaxLength(20);

        builder.Property(x => x.EnvioDirecto)
            .IsRequired();

        // El código de agencia es global.
        builder.HasIndex(x => x.Codigo)
            .IsUnique();

        // Agencia es el aggregate root y contiene sus bases.
        builder.HasMany(x => x.Bases)
            .WithOne()
            .HasForeignKey(x => x.AgenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.DomainEvents);
    }
}