using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// Canal por el que Docuten notifica a un conductor/firmante. Valores admitidos:
/// <c>email</c>, <c>sms</c>, <c>whatsapp</c>. El contacto del conductor debe ser coherente con el
/// canal (<c>email</c> → <c>Email</c>; <c>sms</c>/<c>whatsapp</c> → <c>Movil</c>), invariant que
/// enforce <see cref="Conductores.Conductor.Crear"/>.
/// </summary>
public sealed class Canal : ValueObject
{
    public const string Email = "email";
    public const string Sms = "sms";
    public const string Whatsapp = "whatsapp";

    private static readonly string[] Valores = { Email, Sms, Whatsapp };

    public string Valor { get; }

    // El invariante "Valor siempre en minúsculas" se enforce aquí (no sólo en Create): EF Core
    // reconstruye el VO desde la columna `channel` usando este constructor privado, saltándose
    // Create. Si la BD guarda "SMS"/"Whatsapp" (seed manual SQL), sin esta normalización
    // RequiereMovil/RequiereEmail (comparan Valor == "sms"/"email") devolverían false y
    // ConductorAsignado.TieneCanalValido fallaría → Documento.ConductorSinCanal al confirmar.
    private Canal(string valor) => Valor = valor.ToLowerInvariant();

    /// <summary>Crea un canal validado, o <c>null</c> si <paramref name="raw"/> es vacío/blanco.
    /// Lanza <see cref="ArgumentException"/> si tiene texto pero no es uno de los valores admitidos.</summary>
    public static Canal? Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var valor = raw.Trim().ToLowerInvariant();
        if (Array.IndexOf(Valores, valor) < 0)
        {
            throw new ArgumentException(
                $"El canal '{raw}' no es válido (admitidos: {Email}, {Sms}, {Whatsapp}).", nameof(raw));
        }

        return new Canal(valor);
    }

    /// <summary>True si el canal requiere móvil (<c>sms</c> o <c>whatsapp</c>).</summary>
    public bool RequiereMovil => Valor == Sms || Valor == Whatsapp;

    /// <summary>True si el canal requiere email.</summary>
    public bool RequiereEmail => Valor == Email;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}