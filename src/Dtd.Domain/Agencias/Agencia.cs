using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

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
    public string IdentificadorFiscal { get; private set; }

    public bool Activa { get; private set; }
    public string? AgenciaQs { get; private set; }

    public bool EntregaEnDestino { get; private set; }

    public bool RequierePrecinto { get; private set; }

    private readonly List<AgenciaBase> _bases = [];

    public IReadOnlyCollection<AgenciaBase> Bases => _bases.AsReadOnly();

    /// <summary>
    /// Usado por el ORM para materializar el agregado.
    /// </summary>
    private Agencia()
    {
        Codigo = string.Empty;
        Nombre = string.Empty;
        IdentificadorFiscal = string.Empty;
    }

    private Agencia(
        string codigo,
        string nombre,
        string identificadorFiscal,
        bool activa,
        string? agenciaQs,
        bool entregaEnDestino,
        bool requierePrecinto)
    {
        Id = Guid.NewGuid();

        Codigo = codigo;
        Nombre = nombre;
        IdentificadorFiscal = identificadorFiscal;
        Activa = activa;
        AgenciaQs = agenciaQs;
        EntregaEnDestino = entregaEnDestino;
        RequierePrecinto = requierePrecinto;
    }

    public static Agencia Crear(
        string codigo,
        string nombre,
        string identificadorFiscal,
        string? agenciaQs = null,
        bool entregaEnDestino = false,
        bool requierePrecinto = false)
    {
        ValidarDatos(
            codigo,
            nombre,
            identificadorFiscal);

        return new Agencia(
            codigo.Trim(),
            nombre.Trim(),
            identificadorFiscal.Trim(),
            activa: true,
            agenciaQs?.Trim(),
            entregaEnDestino,
            requierePrecinto);
    }

    public void Desactivar() => Activa = false;

    public void Activar() => Activa = true;

    public void MarcarEntregaEnDestino(bool entregaEnDestino) =>
        EntregaEnDestino = entregaEnDestino;

    public void Modificar(
        string codigo,
        string nombre,
        string identificadorFiscal,
        string? agenciaQs,
        bool entregaEnDestino)
    {
        ValidarDatos(
            codigo,
            nombre,
            identificadorFiscal);

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        IdentificadorFiscal = identificadorFiscal.Trim();
        AgenciaQs = agenciaQs?.Trim();
        EntregaEnDestino = entregaEnDestino;
    }

    public AgenciaBase AgregarBase(
        string codigo,
        string nombre,
        Email? email,
        string language = "es",
        string? direccion = null,
        string? codigoPostal = null,
        string? municipio = null,
        string? codigoPaisIso = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de agencia base es obligatorio.",
                nameof(codigo));
        }

        var codigoNormalizado = codigo.Trim();

        if (_bases.Any(b =>
            b.Codigo.Equals(
                codigoNormalizado,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException(
                $"Ya existe una base con el código '{codigoNormalizado}'.",
                nameof(codigo));
        }

        var baseAgencia = new AgenciaBase(
            Id,
            codigoNormalizado,
            nombre,
            email,
            language,
            direccion,
            codigoPostal,
            municipio,
            codigoPaisIso);

        _bases.Add(baseAgencia);

        return baseAgencia;
    }

    public AgenciaBase ModificarBase(
        Guid baseId,
        string nombre,
        Email? email,
        string language,
        string? direccion = null,
        string? codigoPostal = null,
        string? municipio = null,
        string? codigoPaisIso = null)
    {
        var baseAgencia = _bases
            .FirstOrDefault(b => b.Id == baseId);

        if (baseAgencia is null)
        {
            throw new ArgumentException(
                "La base indicada no pertenece a la agencia.",
                nameof(baseId));
        }

        baseAgencia.Actualizar(
            nombre,
            email,
            language,
            direccion,
            codigoPostal,
            municipio,
            codigoPaisIso);

        return baseAgencia;
    }

    public void EliminarBase(Guid baseId)
    {
        var baseAgencia = _bases
            .FirstOrDefault(b => b.Id == baseId);

        if (baseAgencia is null)
        {
            throw new ArgumentException(
                "La base indicada no pertenece a la agencia.",
                nameof(baseId));
        }

        _bases.Remove(baseAgencia);
    }

    private static void ValidarDatos(
        string codigo,
        string nombre,
        string identificadorFiscal)
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

        if (string.IsNullOrWhiteSpace(identificadorFiscal))
        {
            throw new ArgumentException(
                "El identificador fiscal de la agencia es obligatorio.",
                nameof(identificadorFiscal));
        }
    }
}