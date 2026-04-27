using System;
using System.Collections.Generic;
using System.Text;
using Promociones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Promociones.Infrastructure.Configurations
{
    public class ReglaCategoriaConfiguration : IEntityTypeConfiguration<ReglaCategoria>
    {
        public void Configure(EntityTypeBuilder<ReglaCategoria> builder)
        {
            builder.ToTable("PRO_ReglaCategoria");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedNever();

            builder.Property(r => r.IdPromocionDescuento)
                .IsRequired();

            builder.Property(r => r.IdCategoriaCatalogo)
                .IsRequired();
            // Referencia LÓGICA — sin FK física porque CAT es otro microservicio

            builder.Property(r => r.PorcentajeAplicable)
                .IsRequired()
                .HasPrecision(5, 2);

            // Índice único para evitar reglas duplicadas por categoría en la misma campaña
            builder.HasIndex(r => new { r.IdPromocionDescuento, r.IdCategoriaCatalogo })
                .IsUnique();
        }
    }
}
