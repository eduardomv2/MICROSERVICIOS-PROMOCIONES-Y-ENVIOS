using System;
using System.Collections.Generic;
using System.Text;
using Promociones.Domain.Common;

namespace Promociones.Domain.Entities
{
    public class Promocion
    {
        private static readonly int[] MesesPermitidos = { 3, 6, 9, 12 };

        // Propiedades
        public Guid Id { get; private set; }
        public string NombreCampana { get; private set; } = string.Empty;
        public decimal PorcentajeDescuento { get; private set; }
        public decimal? MontoMinimoCompra { get; private set; }
        public DateTime FechaInicio { get; private set; }
        public DateTime? FechaFin { get; private set; }
        public bool EsIndefinido { get; private set; }

        private List<ReglaCategoria> _reglasCategorias = new();
        public IReadOnlyList<ReglaCategoria> ReglasCategorias => _reglasCategorias.AsReadOnly();

        private List<PromocionMSI> _opcionesMSI = new();
        public IReadOnlyList<PromocionMSI> OpcionesMSI => _opcionesMSI.AsReadOnly();

        protected Promocion() { }

        // Fábrica
        public static ResultadoOperacion<Promocion> Crear(
            string nombreCampana,
            decimal porcentajeDescuento,
            DateTime fechaInicio,
            DateTime? fechaFin,
            bool esIndefinido,
            decimal? montoMinimoCompra = null)
        {
            if (string.IsNullOrWhiteSpace(nombreCampana))
                return ResultadoOperacion<Promocion>.Fallo("El nombre de la campaña es obligatorio");

            if (!PorcentajeEsValido(porcentajeDescuento))
                return ResultadoOperacion<Promocion>.Fallo("El porcentaje debe estar entre 1 y 100");

            if (!esIndefinido && fechaFin == null)
                return ResultadoOperacion<Promocion>.Fallo("Una promoción no indefinida debe tener fecha de fin");

            if (fechaFin.HasValue && fechaFin <= fechaInicio)
                return ResultadoOperacion<Promocion>.Fallo("La fecha de fin debe ser posterior a la de inicio");

            return ResultadoOperacion<Promocion>.Ok(new Promocion
            {
                Id = Guid.NewGuid(),
                NombreCampana = nombreCampana,
                PorcentajeDescuento = porcentajeDescuento,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                EsIndefinido = esIndefinido,
                MontoMinimoCompra = montoMinimoCompra
            });
        }

        // Comportamiento
        public bool EstaVigente()
        {
            var hoy = DateTime.Today;

            if (hoy < FechaInicio) return false;
            if (EsIndefinido) return true;
            if (FechaFin == null) return false;

            return hoy <= FechaFin.Value;
        }

        public ResultadoOperacion AgregarOpcionMSI(int meses, string bancos, decimal montoMinimo)
        {
            if (!MesesPermitidos.Contains(meses))
                return ResultadoOperacion.Fallo("Los meses solo pueden ser 3, 6, 9 o 12");

            if (string.IsNullOrWhiteSpace(bancos))
                return ResultadoOperacion.Fallo("Debe especificar al menos un banco participante");

            if (montoMinimo < 0)
                return ResultadoOperacion.Fallo("El monto mínimo no puede ser negativo");

            _opcionesMSI.Add(PromocionMSI.Crear(Id, meses, bancos, montoMinimo));
            return ResultadoOperacion.Ok();
        }

        public ResultadoOperacion AgregarReglaCategoria(Guid idCategoria, decimal porcentaje)
        {
            if (!PorcentajeEsValido(porcentaje))
                return ResultadoOperacion.Fallo("El porcentaje debe estar entre 1 y 100");

            if (_reglasCategorias.Any(r => r.IdCategoriaCatalogo == idCategoria))
                return ResultadoOperacion.Fallo("Ya existe una regla para esta categoría");

            _reglasCategorias.Add(ReglaCategoria.Crear(Id, idCategoria, porcentaje));
            return ResultadoOperacion.Ok();
        }

        // Métodos privados
        private static bool PorcentajeEsValido(decimal porcentaje) =>
            porcentaje > 0 && porcentaje <= 100;
    }
}
