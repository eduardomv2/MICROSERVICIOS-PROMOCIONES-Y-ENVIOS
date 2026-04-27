using System;
using System.Collections.Generic;
using System.Text;

namespace Promociones.Domain.Common
{
    public class ResultadoOperacion
    {
        public bool EsExitoso { get; private set; }
        public string Error { get; private set; } = string.Empty;

        private ResultadoOperacion() { }

        public static ResultadoOperacion Ok() => new() { EsExitoso = true };
        public static ResultadoOperacion Fallo(string error) => new() { EsExitoso = false, Error = error };
    }

    public class ResultadoOperacion<T>
    {
        public bool EsExitoso { get; private set; }
        public string Error { get; private set; } = string.Empty;
        public T? Valor { get; private set; }

        private ResultadoOperacion() { }

        public static ResultadoOperacion<T> Ok(T valor) => new() { EsExitoso = true, Valor = valor };
        public static ResultadoOperacion<T> Fallo(string error) => new() { EsExitoso = false, Error = error };
    }
}
