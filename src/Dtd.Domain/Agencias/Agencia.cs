using Dtd.Domain.Common;

namespace Dtd.Domain.Agencias;

/// <summary>
/// Agregado de referencia para una agencia de transporte (carrier). Identificada por un código
/// estable **por empresa** (clave natural <c>(empresa, codigo)</c>, igual que <c>Almacen</c> y los
/// documentos), con el código QS externo opcional de la tabla legacy <c>AGENCIAS_QS</c>. El ERP la
/// identifica por <c>carrierId</c> (= <c>codigo</c>, dentro de la empresa). Tiene catálogo de
/// <see cref="Conductores.Conductor"/> (1:N) y se vincula a almacenes vía <c>almacen_agencias</c>.
/// </summary>
public sealed class Agencia : AggregateRoot<Guid>
{
    public string Empresa { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public bool Activa { get; private set; }
    public string? AgenciaQs { get; private set; }

    /// <summary>Marca que indica que los trasiegos de esta agencia se envían <b>directos</b> al almacén
    /// destino (1 envío por almacén destino, agrupando expediciones) en lugar de colapsar todos en un
    /// único envío a la base del carrier. Cuando es <c>true</c>, el DDT no mezcla expediciones de cliente
    /// con trasiegos (sólo trasiegos, agrupados por destino). Default <c>false</c>.</summary>
    public bool EnvioDirecto { get; private set; }

    /// <summary>Usado por el ORM para materializar el agregado; no para código de aplicación.</summary>
    private Agencia()
    {
        Empresa = string.Empty;
        Codigo = string.Empty;
        Nombre = string.Empty;
    }

    private Agencia(
        string empresa, string codigo, string nombre, bool activa, string? agenciaQs, bool envioDirecto)
    {
        Id = Guid.NewGuid();
        Empresa = empresa;
        Codigo = codigo;
        Nombre = nombre;
        Activa = activa;
        AgenciaQs = agenciaQs;
        EnvioDirecto = envioDirecto;
    }

    /// <summary>
    /// Crea una agencia activa. Normaliza la empresa a 3 dígitos y trima los textos. Lanza
    /// <see cref="ArgumentException"/> si la empresa no es un id válido (1–999) o si
    /// <paramref name="codigo"/>/<paramref name="nombre"/> son vacíos.
    /// </summary>
    /// <param name="envioDirecto">Si <c>true</c>, los trasiegos de esta agencia se envían directos al
    /// almacén destino (1 envío por destino) en vez de colapsar en un envío único a la base. Default <c>false</c>.</param>
    public static Agencia Crear(
    string empresa,
    string codigo,
    string nombre,
    string? agenciaQs = null,
    bool envioDirecto = false)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException(
                "La empresa es obligatoria.",
                nameof(empresa));
        }

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
            empresa.Trim(),
            codigo.Trim(),
            nombre.Trim(),
            activa: true,
            agenciaQs?.Trim(),
            envioDirecto);
    }

    public void Desactivar() => Activa = false;
    public void Activar() => Activa = true;

    /// <summary>Marca/desmarca la agencia como envío directo (trasiegos directos al almacén destino).</summary>
    public void MarcarEnvioDirecto(bool envioDirecto) => EnvioDirecto = envioDirecto;
}