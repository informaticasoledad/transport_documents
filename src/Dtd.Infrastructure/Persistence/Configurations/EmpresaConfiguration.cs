using Dtd.Domain.Empresas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("empresa")
            .HasMaxLength(50)
            .IsRequired();

        builder.Ignore(x => x.Codigo);

        builder.Property(x => x.BaseAddress).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TaxId).HasColumnName("tax_id").HasMaxLength(20);
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200);
    }
}
