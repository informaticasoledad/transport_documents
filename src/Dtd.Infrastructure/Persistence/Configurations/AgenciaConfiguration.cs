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

        builder.Property(x => x.Empresa).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Activa);
        builder.Property(x => x.AgenciaQs).HasMaxLength(20);
        // Marca de trasiegos directos al almacén destino (1 envío por destino) vs envío único a la base.
        builder.Property(x => x.EnvioDirecto).IsRequired();

        builder.HasIndex(x => new { x.Empresa, x.Codigo }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
