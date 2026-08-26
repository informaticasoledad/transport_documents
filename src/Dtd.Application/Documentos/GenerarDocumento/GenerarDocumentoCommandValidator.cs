using Dtd.Domain.Empresas;
using FluentValidation;

namespace Dtd.Application.Documentos.GenerarDocumento;

public sealed class GenerarDocumentoCommandValidator : AbstractValidator<GenerarDocumentoCommand>
{
    public GenerarDocumentoCommandValidator()
    {
        RuleFor(x => x.Empresa)
            .Must(Empresa.EsValida)
            .WithMessage("La empresa es obligatoria.");

        RuleFor(x => x.AlmacenId).NotEqual(Guid.Empty).WithMessage("El almacen (Id) es obligatorio.");
        RuleFor(x => x.AgenciaId).NotEqual(Guid.Empty).WithMessage("La agencia (Id) es obligatoria.");
        RuleFor(x => x.FechaDesde).LessThanOrEqualTo(x => x.FechaHasta)
            .WithMessage("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");
    }
}
