using System;
using System.Collections.Generic;
using System.Text;

namespace Envios.Domain.Enums
{
    public enum EstadoEnvio
    {
        Preparando,
        EnCamino,
        Entregado,
        Fallido,
        Reprogramado
    }
}
