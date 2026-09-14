using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Almacenes.EliminarAlmacenAgencia
{
    public sealed record EliminarAlmacenAgenciaCommand(
        string Empresa,
        Guid AlmacenId,
        Guid AgenciaId)
        : IRequest<ErrorOr<Success>>;
}
