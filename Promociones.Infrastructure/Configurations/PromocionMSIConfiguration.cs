using System;
using System.Collections.Generic;
using System.Text;
using Promociones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Promociones.Infrastructure.Configurations
{
    public class PromocionMSIConfiguration : IEntityTypeConfiguration<PromocionMSI>
    {
        public void Configure(EntityTypeBuilder<PromocionMSI> builder)
        {
            builder.ToTable("PRO_PromocionMSI");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .ValueGeneratedNever();

            builder.Property(m => m.IdPromocionDescuento)
                .IsRequired();

            builder.Property(m => m.BancosParticipantes)
                .IsRequired()
                .HasMaxLength(300);
            // Texto plano — ej: "BBVA, Banamex, HSBC"

            builder.Property(m => m.Meses)
                .IsRequired();
            // Solo 3, 6, 9 o 12 — validado en el dominio

            builder.Property(m => m.MontoMinimoCompra)
                .IsRequired()
                .HasPrecision(18, 2);
        }
    }
}
