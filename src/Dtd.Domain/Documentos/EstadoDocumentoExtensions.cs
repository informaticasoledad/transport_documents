using System;
using System.Collections.Generic;
using System.Text;


namespace Dtd.Domain.Documentos
{
    public static class EstadoDocumentoExtensions
    {
        public static bool EsEstadoFinal(this EstadoDocumento estado) =>
            estado is EstadoDocumento.Finalizado
                or EstadoDocumento.Anulado
                or EstadoDocumento.Cancelado;
    }
}