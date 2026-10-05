using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Ccs
{
    public sealed record CcAlmacenAgenciaDto(
        Guid Id,
        string Codigo,
        string Nombre,
        string Email,
        string? Language,
        bool Activo,
        bool PorDefecto);
}