using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Ccs.AgregarCcDefecto
{
    
    public sealed record AgregarCcDefectoCommand(
        string Empresa,
        Guid AlmacenId,
        Guid AgenciaId,
        Guid CcId)
        : IRequest<ErrorOr<CcCatalogoDto>>;

    }