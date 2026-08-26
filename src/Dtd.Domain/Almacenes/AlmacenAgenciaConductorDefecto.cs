using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Almacenes
{
    public sealed class AlmacenAgenciaConductorDefecto
    {
        public Guid AlmacenId { get; set; }
        public Guid AgenciaId { get; set; }
        public Guid ConductorId { get; set; }
    }
}