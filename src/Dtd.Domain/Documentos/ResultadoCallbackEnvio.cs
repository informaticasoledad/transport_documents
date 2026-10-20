using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Documentos
{
    public enum ResultadoCallbackEnvio
    {
        Aplicado,
        YaAplicado,
        Obsoleto,
        EnvioNoEncontrado,
        IdentificadorIncompatible,
        EstadoNoReconocido
    }
}
