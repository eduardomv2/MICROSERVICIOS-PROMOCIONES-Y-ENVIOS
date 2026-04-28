using System;
using System.Collections.Generic;
using System.Text;
using Envios.Domain.Entities;
using Envios.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Envios.Infrastructure.Configurations
{
    public class EnvioConfiguration : IEntityTypeConfiguration<Envio>
    {
        public void Configure(EntityTypeBuilder<Envio> builder)
        {
            builder.ToTable("ENV_Envio");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.IdOrden)
                .IsRequired();

            builder.Property(e => e.DireccionSnapshot)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.GuiaPaqueteria)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.EstadoActual)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(e => e.NombreRepartidor)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.FechaEstimada)
                .IsRequired();

            builder.Property(e => e.FechaEntregado)
                .IsRequired(false);

            // ← FIX: usar propiedad pública directamente
            builder.HasMany(e => e.HistorialRastros)
                .WithOne()
                .HasForeignKey(h => h.IdEnvio)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
