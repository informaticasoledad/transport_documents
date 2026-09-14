using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Almacenes.EliminarCcDefecto
{

    public sealed record EliminarCcDefectoCommand(
        string Empresa,
        Guid AlmacenId,
        Guid AgenciaId,
        Guid CcId)
        : IRequest<ErrorOr<Success>>;
}