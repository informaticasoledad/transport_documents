using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.ActualizarAlmacenAgenciaCc;

public sealed record ActualizarAlmacenAgenciaCcCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid CcId,
    bool PorDefecto
) : IRequest<ErrorOr<Success>>;