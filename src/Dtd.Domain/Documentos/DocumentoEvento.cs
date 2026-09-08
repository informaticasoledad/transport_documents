namespace Dtd.Domain.Documentos;

public sealed class DocumentoEvento
{
    private DocumentoEvento()
    {
    }

    public DocumentoEvento(
        Guid documentoId,
        TipoEventoDocumento tipo,
        EstadoDocumento? estadoAnterior = null,
        EstadoDocumento? estadoNuevo = null,
        string? descripcion = null,
        string? origen = null,
        string? usuario = null,
        Guid? envioId = null)
    {
        Id = Guid.NewGuid();
        DocumentoId = documentoId;
        Fecha = DateTimeOffset.UtcNow;
        Tipo = tipo;
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        Descripcion = descripcion;
        Origen = origen;
        Usuario = usuario;
        EnvioId = envioId;
    }

    public Guid Id { get; private set; }
    public Guid DocumentoId { get; private set; }

    public DateTimeOffset Fecha { get; private set; }

    public TipoEventoDocumento Tipo { get; private set; }

    public EstadoDocumento? EstadoAnterior { get; private set; }
    public EstadoDocumento? EstadoNuevo { get; private set; }

    public string? Descripcion { get; private set; }
    public string? Origen { get; private set; }
    public string? Usuario { get; private set; }

    public Guid? EnvioId { get; private set; }
}