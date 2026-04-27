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
            // Nombre de tabla según documentación de Modelado de Datos
            builder.ToTable("ENV_Envio");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedNever(); // El dominio genera el GUID, no la BD

            builder.Property(e => e.IdOrden)
                .IsRequired();
            // No se crea FK física — es referencia lógica a microservicio de Órdenes

            builder.Property(e => e.DireccionSnapshot)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.GuiaPaqueteria)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.EstadoActual)
                .IsRequired()
                .HasConversion<string>() // Guardamos el enum como string legible
                .HasMaxLength(20);

            builder.Property(e => e.NombreRepartidor)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.FechaEstimada)
                .IsRequired();

            builder.Property(e => e.FechaEntregado)
                .IsRequired(false);

            // Relación con historial — cascade delete
            // Si se elimina un envío, se eliminan sus eventos de rastreo
            builder.HasMany<HistorialRastreo>("_historialRastreo")
                .WithOne()
                .HasForeignKey(h => h.IdEnvio)
                .OnDelete(DeleteBehavior.Cascade);

            // EF Core (Entity Framework Core) accede a la lista privada por nombre
            builder.Navigation("_historialRastreo").AutoInclude();
        }
    }
}
