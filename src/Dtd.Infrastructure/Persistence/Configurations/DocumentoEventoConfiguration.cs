using Dtd.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;


namespace Dtd.Infrastructure.Persistence.Configurations;

internal sealed class DocumentoEventoConfiguration: IEntityTypeConfiguration<DocumentoEvento>
{
    public void Configure(EntityTypeBuilder<DocumentoEvento> builder)
    {
        builder.ToTable("documento_eventos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentoId)
            .IsRequired();

        builder.Property(x => x.Fecha)
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EstadoAnterior)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.EstadoNuevo)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Descripcion)
            .HasMaxLength(500);

        builder.Property(x => x.Origen)
            .HasMaxLength(50);

        builder.Property(x => x.Usuario)
            .HasMaxLength(100);

        builder.Property(x => x.EnvioId);

        builder.HasIndex(x => new
        {
            x.DocumentoId,
            x.Fecha
        });
    }
}