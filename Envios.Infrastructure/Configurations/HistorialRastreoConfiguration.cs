using System;
using System.Collections.Generic;
using System.Text;
using Envios.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Envios.Infrastructure.Configurations
{
    public class HistorialRastreoConfiguration : IEntityTypeConfiguration<HistorialRastreo>
    {
        public void Configure(EntityTypeBuilder<HistorialRastreo> builder)
        {
            builder.ToTable("ENV_HistorialRastreo");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Id)
                .ValueGeneratedNever();

            builder.Property(h => h.IdEnvio)
                .IsRequired();

            builder.Property(h => h.Estado)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(h => h.Nota)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.FechaEvento)
                .IsRequired();

            builder.Property(h => h.NuevaFechaProgramada)
                .IsRequired(false);

            // Sin campos de edición — el historial es INMUTABLE por diseño
            // No hay UpdatedAt, UpdatedBy, etc.
        }
    }
}
