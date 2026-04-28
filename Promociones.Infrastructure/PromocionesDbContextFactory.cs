using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Promociones.Infrastructure
{
    public class PromocionesDbContextFactory : IDesignTimeDbContextFactory<PromocionesDbContext>
    {
        public PromocionesDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<PromocionesDbContext>()
                .UseNpgsql("Host=localhost;Port=5434;Database=PromocionesDB;Username=postgres;Password=admin")
                .Options;

            return new PromocionesDbContext(options);
        }
    }
}
