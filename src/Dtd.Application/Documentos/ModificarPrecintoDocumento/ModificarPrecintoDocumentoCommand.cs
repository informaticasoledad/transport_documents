using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Documentos.ModificarPrecintoDocumento;

public sealed record ModificarPrecintoDocumentoCommand(
    Guid DocumentoId,
    string? Precinto)
    : IRequest<ErrorOr<Success>>;

internal sealed class ModificarPrecintoDocumentoCommandValidator
    : AbstractValidator<ModificarPrecintoDocumentoCommand>
{
    public ModificarPrecintoDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId)
            .NotEmpty();
    }
}