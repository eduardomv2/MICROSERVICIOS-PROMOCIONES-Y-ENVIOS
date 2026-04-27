using System;
using System.Collections.Generic;
using System.Text;

namespace Promociones.Domain.Entities
{
    public class PromocionMSI
    {
        public Guid Id { get; private set; }
        public Guid IdPromocionDescuento { get; private set; }
        public string BancosParticipantes { get; private set; } = string.Empty;
        public int Meses { get; private set; }
        public decimal MontoMinimoCompra { get; private set; }

        private PromocionMSI() { }

        internal static PromocionMSI Crear(
            Guid idPromocion,
            int meses,
            string bancos,
            decimal montoMinimo)
        {
            return new PromocionMSI
            {
                Id = Guid.NewGuid(),
                IdPromocionDescuento = idPromocion,
                BancosParticipantes = bancos,
                Meses = meses,
                MontoMinimoCompra = montoMinimo
            };
        }
    }
}
