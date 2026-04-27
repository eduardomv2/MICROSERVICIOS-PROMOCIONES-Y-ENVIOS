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
                .HasPrecision(5, 2); // ej: 100.00

            builder.Property(p => p.MontoMinimoCompra)
                .IsRequired(false)
                .HasPrecision(18, 2);
            // NULL significa que aplica sin importar el monto total

            builder.Property(p => p.FechaInicio)
                .IsRequired();

            builder.Property(p => p.FechaFin)
                .IsRequired(false);

            builder.Property(p => p.EsIndefinido)
                .IsRequired();
            // Distingue promo sin fin intencional vs sin fecha asignada aún

            // Relaciones con colecciones privadas
            builder.HasMany<ReglaCategoria>("_reglasCategorias")
                .WithOne()
                .HasForeignKey(r => r.IdPromocionDescuento)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<PromocionMSI>("_opcionesMSI")
                .WithOne()
                .HasForeignKey(m => m.IdPromocionDescuento)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation("_reglasCategorias").AutoInclude();
            builder.Navigation("_opcionesMSI").AutoInclude();
        }
    }
}
