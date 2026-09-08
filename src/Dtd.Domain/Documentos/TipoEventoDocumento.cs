using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Documentos
{
    public enum TipoEventoDocumento
    {
        Creado = 1,
        EnviadoPlataforma = 2,
        ReintentoEnvio = 3,
        EstadoCambiado = 4,
        Anulado = 5,
        Cancelado = 6,
        CallbackDocumento = 7,
        CallbackEnvio = 8,
        FirmaRecibida = 9,
        ErrorEnvio = 10
    }
}