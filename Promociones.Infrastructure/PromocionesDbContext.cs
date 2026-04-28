using System;
using System.Collections.Generic;
using System.Text;
using Promociones.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Promociones.Infrastructure
{
    public class PromocionesDbContext : DbContext
    {
        public PromocionesDbContext(DbContextOptions<PromocionesDbContext> options) : base(options) { }

        public DbSet<Promocion> Promociones => Set<Promocion>();
        public DbSet<ReglaCategoria> ReglasCategoria => Set<ReglaCategoria>();
        public DbSet<PromocionMSI> PromocionMSIs => Set<PromocionMSI>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromocionesDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
