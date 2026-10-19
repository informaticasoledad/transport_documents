using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Documentos.ConductoresDocumento;

/// <summary>
/// Añade un conductor manual a un documento en estado Nuevo,
/// sin incorporarlo al catálogo de conductores.
/// </summary>
public sealed record AgregarConductorManualDocumentoCommand(
    Guid DocumentoId,
    string Nombre,
    string TaxId,
    string Movil)
    : IRequest<ErrorOr<ConductorDto>>;
internal sealed class AgregarConductorManualDocumentoCommandValidator
    : AbstractValidator<AgregarConductorManualDocumentoCommand>
{
    public AgregarConductorManualDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId)
            .NotEmpty();

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TaxId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Movil)
            .NotEmpty()
            .WithMessage("El móvil del conductor es obligatorio.");
    }
}
