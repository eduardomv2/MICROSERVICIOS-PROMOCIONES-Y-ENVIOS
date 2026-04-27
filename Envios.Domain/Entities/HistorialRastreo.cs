using System;
using System.Collections.Generic;
using System.Text;

namespace Envios.Domain.Entities
{
    public class HistorialRastreo
    {
        public Guid Id { get; private set; }
        public Guid IdEnvio { get; private set; }
        public string Estado { get; private set; } = string.Empty;
        public string Nota { get; private set; } = string.Empty;
        public DateTime FechaEvento { get; private set; }
        public DateTime? NuevaFechaProgramada { get; private set; }

        private HistorialRastreo() { }

        internal static HistorialRastreo Crear(
            Guid idEnvio,
            string estado,
            string nota,
            DateTime? nuevaFechaProgramada = null)
        {
            return new HistorialRastreo
            {
                Id = Guid.NewGuid(),
                IdEnvio = idEnvio,
                Estado = estado,
                Nota = nota,
                FechaEvento = DateTime.UtcNow,
                NuevaFechaProgramada = nuevaFechaProgramada
            };
        }
    }
}
