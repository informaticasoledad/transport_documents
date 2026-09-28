using FluentValidation;

namespace Dtd.Application.Documentos.ModificarMatriculaDocumento;

internal sealed class ModificarMatriculaDocumentoCommandValidator
    : AbstractValidator<ModificarMatriculaDocumentoCommand>
{
    public ModificarMatriculaDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId)
            .NotEmpty();

        RuleFor(x => x.Matricula)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Matricula));
    }
}