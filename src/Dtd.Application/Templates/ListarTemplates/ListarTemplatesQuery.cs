using ErrorOr;
using MediatR;

namespace Dtd.Application.Templates.ListarTemplates;

public sealed record ListarTemplatesQuery(
    string Empresa)
    : IRequest<ErrorOr<IReadOnlyList<TemplateDto>>>;