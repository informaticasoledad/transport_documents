using Dtd.Application.Ccs;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Almacenes.VincularAlmacenAgenciaCc
{
    public sealed record VincularAlmacenAgenciaCcCommand(
        string Empresa,
        Guid AlmacenId,
        Guid AgenciaId,
        Guid CcId,
        bool PorDefecto)
        : IRequest<ErrorOr<CcCatalogoDto>>;
}