using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Conductores
{
    public sealed class ConductorAgencia
    {
        public Guid ConductorId { get; set; }
        public Guid AgenciaId { get; set; }
    }
}