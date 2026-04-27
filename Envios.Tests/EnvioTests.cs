using System;
using System.Collections.Generic;
using System.Text;
using Envios.Domain.Entities;
using Envios.Domain.Enums;

namespace Envios.Tests
{
    public class EnvioTests
    {
        [Fact]
        public void CrearEnvio_ConDatosValidos_DebeCrearseEnEstadoPreparando()
        {
            var resultado = Envio.Crear(
                idOrden: Guid.NewGuid(),
                direccionSnapshot: "Av. Juárez 123, Centro, Monclova, Coahuila",
                nombreRepartidor: "Juan Pérez",
                fechaEstimada: DateTime.Today.AddDays(3)
            );

            Assert.True(resultado.EsExitoso);
            Assert.Equal(EstadoEnvio.Preparando, resultado.Valor.EstadoActual);
        }

        [Fact]
        public void CrearEnvio_SinDireccionSnapshot_DebeRechazarse()
        {
            var resultado = Envio.Crear(
                idOrden: Guid.NewGuid(),
                direccionSnapshot: "",
                nombreRepartidor: "Juan Pérez",
                fechaEstimada: DateTime.Today.AddDays(3)
            );

            Assert.False(resultado.EsExitoso);
            Assert.Contains("dirección", resultado.Error);
        }

        [Fact]
        public void CambiarEstado_DePreparandoAEnCamino_DebePermitirse()
        {
            var envio = CrearEnvioValido();
            var resultado = envio.CambiarEstado(EstadoEnvio.EnCamino, "Salió del almacén");

            Assert.True(resultado.EsExitoso);
            Assert.Equal(EstadoEnvio.EnCamino, envio.EstadoActual);
        }

        [Fact]
        public void CambiarEstado_DePreparandoAEntregado_DebeRechazarse()
        {
            var envio = CrearEnvioValido();
            var resultado = envio.CambiarEstado(EstadoEnvio.Entregado, "");

            Assert.False(resultado.EsExitoso);
            Assert.Contains("Transición no permitida", resultado.Error);
        }

        [Fact]
        public void CambiarEstado_DeEnCaminoAEntregado_DebePermitirse()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            var resultado = envio.CambiarEstado(EstadoEnvio.Entregado, "Entregado");

            Assert.True(resultado.EsExitoso);
            Assert.Equal(EstadoEnvio.Entregado, envio.EstadoActual);
        }

        [Fact]
        public void CambiarEstado_DeEnCaminoAFallido_DebePermitirse()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            var resultado = envio.CambiarEstado(EstadoEnvio.Fallido, "No había nadie");

            Assert.True(resultado.EsExitoso);
            Assert.Equal(EstadoEnvio.Fallido, envio.EstadoActual);
        }

        [Fact]
        public void CambiarEstado_DeEntregadoACualquierEstado_DebeRechazarse()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            envio.CambiarEstado(EstadoEnvio.Entregado, "Entregado");
            var resultado = envio.CambiarEstado(EstadoEnvio.EnCamino, "");

            Assert.False(resultado.EsExitoso);
        }

        [Fact]
        public void Reprogramar_DesdeEstadoFallido_DebePermitirse()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            envio.CambiarEstado(EstadoEnvio.Fallido, "No había nadie");
            var resultado = envio.Reprogramar(DateTime.Today.AddDays(2), "Cliente contactado");

            Assert.True(resultado.EsExitoso);
            Assert.Equal(EstadoEnvio.Reprogramado, envio.EstadoActual);
        }

        [Fact]
        public void Reprogramar_DesdeEstadoEnCamino_DebeRechazarse()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            var resultado = envio.Reprogramar(DateTime.Today.AddDays(2), "");

            Assert.False(resultado.EsExitoso);
            Assert.Contains("estado 'fallido'", resultado.Error);
        }

        [Fact]
        public void Reprogramar_ConFechaEnElPasado_DebeRechazarse()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            envio.CambiarEstado(EstadoEnvio.Fallido, "Falló");
            var resultado = envio.Reprogramar(DateTime.Today.AddDays(-1), "");

            Assert.False(resultado.EsExitoso);
            Assert.Contains("fecha futura", resultado.Error);
        }

        [Fact]
        public void FechaEntregado_SoloSeAsignaCuandoEstadoEsEntregado()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            envio.CambiarEstado(EstadoEnvio.Entregado, "Entregado");

            Assert.NotNull(envio.FechaEntregado);
        }

        [Fact]
        public void FechaEntregado_NoDebeAsignarseEnOtrosEstados()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");

            Assert.Null(envio.FechaEntregado);
        }

        [Fact]
        public void HistorialRastreo_AlCrearEnvio_DebeIniciarConUnEvento()
        {
            var envio = CrearEnvioValido();
            Assert.Single(envio.HistorialRastros);
        }

        [Fact]
        public void HistorialRastreo_CadaCambioDeEstado_AgregaUnEvento()
        {
            var envio = CrearEnvioValido();
            envio.CambiarEstado(EstadoEnvio.EnCamino, "En camino");
            envio.CambiarEstado(EstadoEnvio.Entregado, "Entregado");

            Assert.Equal(3, envio.HistorialRastros.Count);
        }

        // Helper para no repetir la creación en cada test
        private Envio CrearEnvioValido()
        {
            var resultado = Envio.Crear(
                idOrden: Guid.NewGuid(),
                direccionSnapshot: "Av. Juárez 123, Centro, Monclova, Coahuila",
                nombreRepartidor: "Juan Pérez",
                fechaEstimada: DateTime.Today.AddDays(3)
            );
            return resultado.Valor!;
        }
    }
}
