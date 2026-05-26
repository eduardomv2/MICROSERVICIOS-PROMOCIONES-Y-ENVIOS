using System;
using System.Collections.Generic;
using System.Text;
using Envios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Envios.Infrastructure
{
    public class EnviosDbContext : DbContext
    {
        public EnviosDbContext(DbContextOptions<EnviosDbContext> options) : base(options) { }
        public DbSet<Envio> Envios => Set<Envio>();
        public DbSet<HistorialRastreo> HistorialRastreos => Set<HistorialRastreo>();
        public DbSet<Repartidor> Repartidores => Set<Repartidor>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnviosDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}