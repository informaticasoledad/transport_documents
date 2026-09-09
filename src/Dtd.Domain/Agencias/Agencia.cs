using Dtd.Domain.Common;

namespace Dtd.Domain.Agencias;

/// <summary>
/// Agregado de referencia para una agencia de transporte (carrier).
/// Se identifica por un código estable global y puede tener un código QS externo opcional
/// procedente de la tabla legacy AGENCIAS_QS.
/// 
/// La agencia se vincula a almacenes mediante almacen_agencias y puede tener
/// un catálogo de conductores y bases asociadas.
/// </summary>
public sealed class Agencia : AggregateRoot<Guid>
{
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public bool Activa { get; private set; }
    public string? AgenciaQs { get; private set; }

    /// <summary>
    /// Indica que los trasiegos de esta agencia se envían directamente al almacén destino
    /// (1 envío por almacén destino, agrupando expediciones) en lugar de colapsarlos
    /// en un único envío a la base del carrier.
    /// </summary>
    public bool EnvioDirecto { get; private set; }

    private readonly List<AgenciaBase> _bases = [];

    public IReadOnlyCollection<AgenciaBase> Bases => _bases.AsReadOnly();

    /// <summary>
    /// Usado por el ORM para materializar el agregado.
    /// </summary>
    private Agencia()
    {
        Codigo = string.Empty;
        Nombre = string.Empty;
    }

    private Agencia(
        string codigo,
        string nombre,
        bool activa,
        string? agenciaQs,
        bool envioDirecto)
    {
        Id = Guid.NewGuid();
        Codigo = codigo;
        Nombre = nombre;
        Activa = activa;
        AgenciaQs = agenciaQs;
        EnvioDirecto = envioDirecto;
    }

    public static Agencia Crear(
        string codigo,
        string nombre,
        string? agenciaQs = null,
        bool envioDirecto = false)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de agencia es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de agencia es obligatorio.",
                nameof(nombre));
        }

        return new Agencia(
            codigo.Trim(),
            nombre.Trim(),
            activa: true,
            agenciaQs?.Trim(),
            envioDirecto);
    }

    public void Desactivar() => Activa = false;

    public void Activar() => Activa = true;

    public void MarcarEnvioDirecto(bool envioDirecto) =>
        EnvioDirecto = envioDirecto;

    public void Modificar(
        string codigo,
        string nombre,
        string? agenciaQs,
        bool envioDirecto)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de agencia es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre de agencia es obligatorio.",
                nameof(nombre));
        }

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        AgenciaQs = agenciaQs?.Trim();
        EnvioDirecto = envioDirecto;
    }
}