using Envios.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Envios.Infrastructure.Configurations
{
    public class RepartidorConfiguration : IEntityTypeConfiguration<Repartidor>
    {
        public void Configure(EntityTypeBuilder<Repartidor> builder)
        {
            builder.ToTable("ENV_Repartidor");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Nombre)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(r => r.Telefono)
                .IsRequired()
                .HasMaxLength(20);
            builder.Property(r => r.EstaDisponible)
                .IsRequired();
            builder.Property(r => r.Status)
                .IsRequired();
        }
    }
}