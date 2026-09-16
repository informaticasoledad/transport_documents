namespace Dtd.Application.Templates;

public sealed record TemplateDto(
    Guid Id,
    string Empresa,
    string Code,
    string DocumentType,
    string Name,
    string Language,
    bool Active);