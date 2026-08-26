namespace Dtd.Application.GatewayContracts;

/// <summary>
/// Opciones de **mapeo** Docuten visibles para la capa de Application (el <see cref="IDocutenGateway"/>
/// HTTP y su <c>DocutenOptions</c> viven en Infrastructure; Application no puede depender de ellos).
/// Se bindea en Infrastructure desde la sección <c>Docuten</c> de <c>appsettings.json</c> y se registra
/// como singleton plano (sin <c>IOptions</c> en Application).
/// </summary>
public sealed class DocutenMappingOptions
{
    /// <summary>URL de callback a la que Docuten notifica los cambios de estado del lote/shipments.
    /// Opcional mientras se sondee; vacío/null = no se envía callback_url.</summary>
    public string? CallbackUrl { get; set; }

    /// <summary>Idioma por defecto de los shipments/parties (Docuten <c>language</c>).</summary>
    public string DefaultLanguage { get; set; } = "es";
}