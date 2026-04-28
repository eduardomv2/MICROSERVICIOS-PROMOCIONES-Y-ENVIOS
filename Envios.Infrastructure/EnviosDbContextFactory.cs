using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Envios.Infrastructure
{
    // Esta clase solo la usa el CLI de EF Core para generar migraciones
    // No se usa en tiempo de ejecución
    public class EnviosDbContextFactory : IDesignTimeDbContextFactory<EnviosDbContext>
    {
        public EnviosDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<EnviosDbContext>()
                .UseNpgsql("Host=localhost;Port=5433;Database=EnviosDB;Username=postgres;Password=admin")
                .Options;

            return new EnviosDbContext(options);
        }
    }
}
