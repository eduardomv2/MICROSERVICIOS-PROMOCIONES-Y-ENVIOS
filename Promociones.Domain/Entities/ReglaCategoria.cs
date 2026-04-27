using System;
using System.Collections.Generic;
using System.Text;

namespace Promociones.Domain.Entities
{
    public class ReglaCategoria
    {
        public Guid Id { get; private set; }
        public Guid IdPromocionDescuento { get; private set; }
        public Guid IdCategoriaCatalogo { get; private set; }
        public decimal PorcentajeAplicable { get; private set; }

        private ReglaCategoria() { }

        internal static ReglaCategoria Crear(
            Guid idPromocion,
            Guid idCategoria,
            decimal porcentaje)
        {
            return new ReglaCategoria
            {
                Id = Guid.NewGuid(),
                IdPromocionDescuento = idPromocion,
                IdCategoriaCatalogo = idCategoria,
                PorcentajeAplicable = porcentaje
            };
        }
    }
}
