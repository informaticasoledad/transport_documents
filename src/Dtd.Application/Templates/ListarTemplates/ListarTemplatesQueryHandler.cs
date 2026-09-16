using Dtd.Domain.Templates;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Templates.ListarTemplates;

internal sealed class ListarTemplatesQueryHandler
    : IRequestHandler<
        ListarTemplatesQuery,
        ErrorOr<IReadOnlyList<TemplateDto>>>
{
    private readonly ITemplateRepository _templateRepository;

    public ListarTemplatesQueryHandler(
        ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<TemplateDto>>> Handle(
        ListarTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Empresa))
        {
            return Error.Validation(
                "Template.EmpresaObligatoria",
                "La empresa es obligatoria.");
        }

        var templates =
            await _templateRepository.ListarPorEmpresaAsync(
                request.Empresa.Trim(),
                cancellationToken);

        return templates
            .Select(t => new TemplateDto(
                t.Id,
                t.Empresa,
                t.Code,
                t.DocumentType,
                t.Name,
                t.Language,
                t.Active))
            .ToList();
    }
}