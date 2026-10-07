using ErrorOr;
using MediatR;

namespace Dtd.Application.Almacenes.DesvincularAlmacenAgenciaCc;

public sealed record DesvincularAlmacenAgenciaCcCommand(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId,
    Guid CcId)
    : IRequest<ErrorOr<Success>>;