using System;
using System.Collections.Generic;
using System.Text;
using Promociones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Promociones.Infrastructure.Configurations
{
    public class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
    {
        public void Configure(EntityTypeBuilder<Promocion> builder)
        {
            builder.ToTable("PRO_Descuento");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.NombreCampana)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.PorcentajeDescuento)
                .IsRequired()
                .HasPrecision(5, 2);

            builder.Property(p => p.MontoMinimoCompra)
                .IsRequired(false)
                .HasPrecision(18, 2);

            builder.Property(p => p.FechaInicio)
                .IsRequired();

            builder.Property(p => p.FechaFin)
                .IsRequired(false);

            builder.Property(p => p.EsIndefinido)
                .IsRequired();

            // ← FIX: usar propiedades públicas directamente
            builder.HasMany(p => p.ReglasCategorias)
                .WithOne()
                .HasForeignKey(r => r.IdPromocionDescuento)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.OpcionesMSI)
                .WithOne()
                .HasForeignKey(m => m.IdPromocionDescuento)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
