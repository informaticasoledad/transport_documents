using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.AgenciaBases;

/// <summary>
/// Base logistica de una agencia. Pertenece a una empresa y contiene la direccion/contacto usado como
/// destino cuando una agencia agrupa el envio de un almacen concreto en una base propia.
/// </summary>
public sealed class AgenciaBase : AggregateRoot<Guid>
{
    public string Empresa { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public string? TaxId { get; private set; }
    public string? Direccion { get; private set; }
    public string? CodigoPostal { get; private set; }
    public string? Municipio { get; private set; }
    public string? CodigoPaisIso { get; private set; }
    public Movil? Movil { get; private set; }
    public Email? Email { get; private set; }
    public Canal Canal { get; private set; }
    public string Language { get; private set; }
    public bool Activo { get; private set; }

    /// <summary>Usado por el ORM para materializar el agregado; no para código de aplicación.</summary>
    private AgenciaBase()
    {
        Empresa = string.Empty;
        Codigo = string.Empty;
        Nombre = string.Empty;
        Canal = null!;
        Language = "es";
    }

    private AgenciaBase(
        string empresa, string codigo, string nombre, string? taxId,
        string? direccion, string? codigoPostal, string? municipio, string? codigoPaisIso,
        Movil? movil, Email? email, Canal canal, string language, bool activo)
    {
        Id = Guid.NewGuid();
        Empresa = empresa;
        Codigo = codigo;
        Nombre = nombre;
        TaxId = taxId;
        Direccion = NormalizarOpcional(direccion);
        CodigoPostal = NormalizarOpcional(codigoPostal);
        Municipio = NormalizarOpcional(municipio);
        CodigoPaisIso = NormalizarOpcional(codigoPaisIso);
        Movil = movil;
        Email = email;
        Canal = canal;
        Language = language;
        Activo = activo;
    }

    /// <summary>
    /// Crea una agencia base activa. Trima los textos y valida la coherencia canal-contacto
    /// (<paramref name="channel"/> = <c>email</c> → <paramref name="email"/> obligatorio;
    /// <c>sms</c>/<c>whatsapp</c> → <paramref name="movil"/> obligatorio). Lanza
    /// <see cref="ArgumentException"/> si faltan datos obligatorios o el contacto no corresponde al canal.
    /// </summary>
    public static AgenciaBase Crear(
        string empresa, string codigo, string nombre, Canal channel,
        Movil? movil, Email? email,
        string? taxId = null, string language = "es",
        string? direccion = null, string? codigoPostal = null, string? municipio = null, string? codigoPaisIso = null)
    {
        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException("La empresa es obligatoria.", nameof(empresa));
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de agenciaBase es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de agencia base es obligatorio.", nameof(nombre));
        }

        ArgumentNullException.ThrowIfNull(channel);

        if (channel.RequiereEmail && email is null)
        {
            throw new ArgumentException(
                $"El canal '{channel.Valor}' requiere un email de contacto.", nameof(email));
        }

        if (channel.RequiereMovil && movil is null)
        {
            throw new ArgumentException(
                $"El canal '{channel.Valor}' requiere un móvil de contacto.", nameof(movil));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        return new AgenciaBase(
            empresa.Trim(), codigo.Trim(), nombre.Trim(), taxId?.Trim(),
            direccion, codigoPostal, municipio, codigoPaisIso,
            movil, email, channel, language.Trim(), activo: true);
    }

    /// <summary>
    /// Actualiza los campos mutables de la agencia base (gestión por API). <c>Empresa</c> y <c>Codigo</c> son
    /// inmutables (identificadores) y no se tocan. Re-valida la coherencia canal-contacto, igual que
    /// <see cref="Crear"/>. No cambia <c>Activo</c> (se gestiona con <see cref="Activar"/>/
    /// <see cref="Desactivar"/>).
    /// </summary>
    public void Actualizar(
        string nombre,
        string? taxId,
        Canal channel,
        Movil? movil,
        Email? email,
        string language,
        string? direccion = null,
        string? codigoPostal = null,
        string? municipio = null,
        string? codigoPaisIso = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de agencia base es obligatorio.", nameof(nombre));
        }

        ArgumentNullException.ThrowIfNull(channel);

        if (channel.RequiereEmail && email is null)
        {
            throw new ArgumentException(
                $"El canal '{channel.Valor}' requiere un email de contacto.", nameof(email));
        }

        if (channel.RequiereMovil && movil is null)
        {
            throw new ArgumentException(
                $"El canal '{channel.Valor}' requiere un móvil de contacto.", nameof(movil));
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            language = "es";
        }

        Nombre = nombre.Trim();
        TaxId = taxId?.Trim();
        Direccion = NormalizarOpcional(direccion);
        CodigoPostal = NormalizarOpcional(codigoPostal);
        Municipio = NormalizarOpcional(municipio);
        CodigoPaisIso = NormalizarOpcional(codigoPaisIso);
        Canal = channel;
        Movil = movil;
        Email = email;
        Language = language.Trim();
    }

    public bool TieneDireccionCompleta =>
        !string.IsNullOrWhiteSpace(Direccion) &&
        !string.IsNullOrWhiteSpace(CodigoPostal) &&
        !string.IsNullOrWhiteSpace(Municipio) &&
        !string.IsNullOrWhiteSpace(CodigoPaisIso);

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
