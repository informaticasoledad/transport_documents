using Dtd.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Empresas
{
    public sealed class Empresa : AggregateRoot<string>
    {
        public string Codigo => Id;
        public string BaseAddress { get; private set; } = string.Empty;

        /// <summary>Tax id (CIF/NIF) de la empresa = consignor/cargador del lote de Docuten. Opcional hasta sembrarlo.</summary>
        public string? TaxId { get; private set; }

        /// <summary>Nombre legal de la empresa = nombre del consignor. Opcional hasta sembrarlo.</summary>
        public string? Nombre { get; private set; }

        private Empresa() { } // materialización EF Core

        public static bool EsValida(string? codigo) => !string.IsNullOrWhiteSpace(codigo);
    }
}
