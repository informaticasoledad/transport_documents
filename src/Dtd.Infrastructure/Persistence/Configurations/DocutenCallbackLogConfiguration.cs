using Dtd.Infrastructure.Persistence.IntegrationLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class DocutenCallbackLogConfiguration
    : IEntityTypeConfiguration<DocutenCallbackLog>
{
    public void Configure(EntityTypeBuilder<DocutenCallbackLog> builder)
    {
        builder.ToTable("docuten_callback_logs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecibidoEn).IsRequired();
        builder.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LotId).HasMaxLength(100);
        builder.Property(x => x.LotReference).HasMaxLength(150);
        builder.Property(x => x.ShipmentId).HasMaxLength(100);
        builder.Property(x => x.ShipmentReference).HasMaxLength(150);
        builder.Property(x => x.Event).HasMaxLength(50);
        builder.Property(x => x.Estado).HasMaxLength(50);
        builder.Property(x => x.Procesado).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Headers).HasColumnType("text");
        builder.Property(x => x.Mensaje).HasMaxLength(500);

        builder.HasIndex(x => x.RecibidoEn);
        builder.HasIndex(x => x.DocumentoId);
        builder.HasIndex(x => x.LotId);
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => x.ShipmentReference);
    }
}
