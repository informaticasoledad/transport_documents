namespace Dtd.Application.Templates;

internal sealed class DocumentTemplateValuesBuilderResolver
    : IDocumentTemplateValuesBuilderResolver
{
    private readonly IReadOnlyDictionary<string, IDocumentTemplateValuesBuilder> _builders;

    public DocumentTemplateValuesBuilderResolver(
        IEnumerable<IDocumentTemplateValuesBuilder> builders)
    {
        _builders = builders.ToDictionary(
            x => x.DocumentType,
            StringComparer.OrdinalIgnoreCase);
    }

    public IDocumentTemplateValuesBuilder Resolve(string documentType)
    {
        if (_builders.TryGetValue(documentType, out var builder))
        {
            return builder;
        }

        throw new InvalidOperationException(
            $"No existe un builder de template para el document_type '{documentType}'.");
    }
}