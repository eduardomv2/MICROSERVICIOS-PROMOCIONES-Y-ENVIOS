using System;
using System.Collections.Generic;
using System.Text;
using Envios.Domain.Common;
using Envios.Domain.Enums;

namespace Envios.Domain.Entities
{
    public class Envio
    {
        // Transiciones válidas como constante estática
        private static readonly Dictionary<EstadoEnvio, EstadoEnvio[]> TransicionesValidas = new()
        {
            { EstadoEnvio.Preparando,   new[] { EstadoEnvio.EnCamino } },
            { EstadoEnvio.EnCamino,     new[] { EstadoEnvio.Entregado, EstadoEnvio.Fallido } },
            { EstadoEnvio.Fallido,      new[] { EstadoEnvio.Reprogramado } },
            { EstadoEnvio.Reprogramado, new[] { EstadoEnvio.EnCamino } },
            { EstadoEnvio.Entregado,    Array.Empty<EstadoEnvio>() }
        };

        // Propiedades
        public Guid Id { get; private set; }
        public Guid IdOrden { get; private set; }
        public string DireccionSnapshot { get; private set; } = string.Empty;
        public string GuiaPaqueteria { get; private set; } = string.Empty;
        public EstadoEnvio EstadoActual { get; private set; }
        public string NombreRepartidor { get; private set; } = string.Empty;
        public DateTime FechaEstimada { get; private set; }
        public DateTime? FechaEntregado { get; private set; }

        private List<HistorialRastreo> _historialRastreo = new();
        public IReadOnlyList<HistorialRastreo> HistorialRastros => _historialRastreo.AsReadOnly();

        protected Envio() { }

        // Fábrica
        public static ResultadoOperacion<Envio> Crear(
            Guid idOrden,
            string direccionSnapshot,
            string nombreRepartidor,
            DateTime fechaEstimada)
        {
            if (string.IsNullOrWhiteSpace(direccionSnapshot))
                return ResultadoOperacion<Envio>.Fallo("La dirección no puede estar vacía");

            if (string.IsNullOrWhiteSpace(nombreRepartidor))
                return ResultadoOperacion<Envio>.Fallo("El nombre del repartidor es obligatorio");

            if (fechaEstimada.Date < DateTime.Today)
                return ResultadoOperacion<Envio>.Fallo("La fecha estimada no puede ser en el pasado");

            var envio = new Envio
            {
                Id = Guid.NewGuid(),
                IdOrden = idOrden,
                DireccionSnapshot = direccionSnapshot,
                NombreRepartidor = nombreRepartidor,
                FechaEstimada = fechaEstimada,
                EstadoActual = EstadoEnvio.Preparando,
                GuiaPaqueteria = GenerarGuia()
            };

            envio.RegistrarEvento("Envío creado y en preparación");
            return ResultadoOperacion<Envio>.Ok(envio);
        }

        // Comportamiento
        public ResultadoOperacion CambiarEstado(EstadoEnvio nuevoEstado, string nota)
        {
            if (!TransicionesValidas[EstadoActual].Contains(nuevoEstado))
                return ResultadoOperacion.Fallo(
                    $"Transición no permitida: no se puede pasar de '{EstadoActual}' a '{nuevoEstado}'");

            EstadoActual = nuevoEstado;

            if (nuevoEstado == EstadoEnvio.Entregado)
                FechaEntregado = DateTime.UtcNow;

            RegistrarEvento(nota);
            return ResultadoOperacion.Ok();
        }

        public ResultadoOperacion Reprogramar(DateTime nuevaFecha, string motivo)
        {
            if (EstadoActual != EstadoEnvio.Fallido)
                return ResultadoOperacion.Fallo("Solo se puede reprogramar un envío en estado 'fallido'");

            if (nuevaFecha.Date <= DateTime.Today)
                return ResultadoOperacion.Fallo("La nueva fecha debe ser una fecha futura");

            EstadoActual = EstadoEnvio.Reprogramado;
            FechaEstimada = nuevaFecha;

            _historialRastreo.Add(HistorialRastreo.Crear(Id, "Reprogramado", motivo, nuevaFecha));
            return ResultadoOperacion.Ok();
        }

        public void AgregarEventoRastreo(string nota, DateTime? nuevaFechaProgramada) =>
            _historialRastreo.Add(HistorialRastreo.Crear(Id, EstadoActual.ToString(), nota, nuevaFechaProgramada));

        // Métodos privados
        private void RegistrarEvento(string nota) =>
            _historialRastreo.Add(HistorialRastreo.Crear(Id, EstadoActual.ToString(), nota));

        private static string GenerarGuia() =>
            $"ENV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
    }
}
