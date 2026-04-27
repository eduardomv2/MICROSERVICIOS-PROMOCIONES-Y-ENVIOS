using System;
using System.Collections.Generic;
using System.Text;
using Promociones.Domain.Entities;

namespace Promociones.Tests
{
    public class PromocionTests
    {
        // CREACIÓN DE CAMPAÑA
        [Fact]
        public void CrearPromocion_ConDatosValidos_DebeCrearseCorrecto()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Buen Fin 2025",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today,
                fechaFin: DateTime.Today.AddDays(5),
                esIndefinido: false
            );

            Assert.True(resultado.EsExitoso);
            Assert.Equal("Buen Fin 2025", resultado.Valor!.NombreCampana);
        }

        [Fact]
        public void CrearPromocion_SinNombre_DebeRechazarse()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today,
                fechaFin: DateTime.Today.AddDays(5),
                esIndefinido: false
            );

            Assert.False(resultado.EsExitoso);
            Assert.Contains("nombre", resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(101)]
        public void CrearPromocion_ConPorcentajeInvalido_DebeRechazarse(decimal porcentaje)
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Promo Test",
                porcentajeDescuento: porcentaje,
                fechaInicio: DateTime.Today,
                fechaFin: DateTime.Today.AddDays(5),
                esIndefinido: false
            );

            Assert.False(resultado.EsExitoso);
            Assert.Contains("porcentaje", resultado.Error);
        }

        [Fact]
        public void CrearPromocion_NoIndefinida_SinFechaFin_DebeRechazarse()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Promo Test",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today,
                fechaFin: null,
                esIndefinido: false
            );

            Assert.False(resultado.EsExitoso);
            Assert.Contains("fecha de fin", resultado.Error);
        }

        [Fact]
        public void CrearPromocion_FechaFinMenorAInicio_DebeRechazarse()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Promo Test",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today.AddDays(5),
                fechaFin: DateTime.Today,
                esIndefinido: false
            );

            Assert.False(resultado.EsExitoso);
            Assert.Contains("posterior", resultado.Error);
        }

        // VIGENCIA
        [Fact]
        public void EstaVigente_EsIndefinidoTrue_SinFechaFin_DebeSerVigente()
        {
            var promo = CrearPromocionIndefinida();
            Assert.True(promo.EstaVigente());
        }

        [Fact]
        public void EstaVigente_FechaFinPasada_NoDebeSerVigente()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Promo Vieja",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today.AddDays(-10),
                fechaFin: DateTime.Today.AddDays(-1),
                esIndefinido: false
            );

            Assert.True(resultado.EsExitoso);
            Assert.False(resultado.Valor!.EstaVigente());
        }

        [Fact]
        public void EstaVigente_DentroDelRangoDeFechas_DebeSerVigente()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Buen Fin",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today.AddDays(-2),
                fechaFin: DateTime.Today.AddDays(2),
                esIndefinido: false
            );

            Assert.True(resultado.EsExitoso);
            Assert.True(resultado.Valor!.EstaVigente());
        }

        [Fact]
        public void EstaVigente_FechaInicioFutura_NoDebeSerVigente()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Promo Futura",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today.AddDays(5),
                fechaFin: DateTime.Today.AddDays(10),
                esIndefinido: false
            );

            Assert.True(resultado.EsExitoso);
            Assert.False(resultado.Valor!.EstaVigente());
        }

        // MSI
        [Theory]
        [InlineData(3)]
        [InlineData(6)]
        [InlineData(9)]
        [InlineData(12)]
        public void AgregarMSI_ConMesesValidos_DebePermitirse(int meses)
        {
            var promo = CrearPromocionIndefinida();
            var resultado = promo.AgregarOpcionMSI(meses, "BBVA, Banamex", 3000);

            Assert.True(resultado.EsExitoso);
            Assert.Single(promo.OpcionesMSI);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(24)]
        [InlineData(0)]
        [InlineData(-6)]
        public void AgregarMSI_ConMesesInvalidos_DebeRechazarse(int meses)
        {
            var promo = CrearPromocionIndefinida();
            var resultado = promo.AgregarOpcionMSI(meses, "BBVA", 3000);

            Assert.False(resultado.EsExitoso);
            Assert.Contains("3, 6, 9 o 12", resultado.Error);
        }

        [Fact]
        public void AgregarMSI_SinBancos_DebeRechazarse()
        {
            var promo = CrearPromocionIndefinida();
            var resultado = promo.AgregarOpcionMSI(6, "", 3000);

            Assert.False(resultado.EsExitoso);
            Assert.Contains("banco", resultado.Error);
        }

        [Fact]
        public void AgregarMSI_ConMontoNegativo_DebeRechazarse()
        {
            var promo = CrearPromocionIndefinida();
            var resultado = promo.AgregarOpcionMSI(6, "BBVA", -100);

            Assert.False(resultado.EsExitoso);
            Assert.Contains("negativo", resultado.Error);
        }

        [Fact]
        public void AgregarMSI_VariasOpciones_DebenAcumularse()
        {
            var promo = CrearPromocionIndefinida();
            promo.AgregarOpcionMSI(3, "BBVA", 1000);
            promo.AgregarOpcionMSI(6, "BBVA", 2000);
            promo.AgregarOpcionMSI(12, "BBVA", 5000);

            Assert.Equal(3, promo.OpcionesMSI.Count);
        }

        // REGLAS DE CATEGORÍA
        [Fact]
        public void AgregarReglaCategoria_ConPorcentajeValido_DebePermitirse()
        {
            var promo = CrearPromocionIndefinida();
            var resultado = promo.AgregarReglaCategoria(Guid.NewGuid(), 15);

            Assert.True(resultado.EsExitoso);
            Assert.Single(promo.ReglasCategorias);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(101)]
        public void AgregarReglaCategoria_ConPorcentajeInvalido_DebeRechazarse(decimal porcentaje)
        {
            var promo = CrearPromocionIndefinida();
            var resultado = promo.AgregarReglaCategoria(Guid.NewGuid(), porcentaje);

            Assert.False(resultado.EsExitoso);
            Assert.Contains("entre 1 y 100", resultado.Error);
        }

        [Fact]
        public void AgregarReglaCategoria_MismaCategoriaDosVeces_DebeRechazarse()
        {
            var promo = CrearPromocionIndefinida();
            var idCategoria = Guid.NewGuid();

            promo.AgregarReglaCategoria(idCategoria, 10);
            var resultado = promo.AgregarReglaCategoria(idCategoria, 15);

            Assert.False(resultado.EsExitoso);
            Assert.Contains("Ya existe", resultado.Error);
        }

        [Fact]
        public void AgregarReglaCategoria_CategoriasDistintas_DebenAcumularse()
        {
            var promo = CrearPromocionIndefinida();
            promo.AgregarReglaCategoria(Guid.NewGuid(), 5);
            promo.AgregarReglaCategoria(Guid.NewGuid(), 10);

            Assert.Equal(2, promo.ReglasCategorias.Count);
        }

        // HELPER
        private Promocion CrearPromocionIndefinida()
        {
            var resultado = Promocion.Crear(
                nombreCampana: "Descuento Permanente",
                porcentajeDescuento: 10,
                fechaInicio: DateTime.Today.AddDays(-1),
                fechaFin: null,
                esIndefinido: true
            );
            return resultado.Valor!;
        }
    }
}
