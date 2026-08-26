using Dtd.Domain.Ccs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class CcConfiguration : IEntityTypeConfiguration<Cc>
{
    public void Configure(EntityTypeBuilder<Cc> builder)
    {
        builder.ToTable("ccs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Empresa).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Activo).IsRequired();

        builder.OwnsOne(x => x.Email, e =>
        {
            e.Property(v => v.Valor).HasColumnName("email").HasMaxLength(200).IsRequired();
        });

        builder.HasIndex(x => new { x.Empresa, x.Codigo }).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
