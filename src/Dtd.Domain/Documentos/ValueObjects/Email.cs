using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// Email de un transportista. Junto con el <see cref="Movil"/> es el canal por el que Docuten notifica al
/// carrier; basta con uno de los dos para transmitir el documento (invariante "móvil o email"). Se guarda
/// normalizado (trim + minúsculas) para que el lookup del default y el valor del ERP sean comparables.
/// </summary>
public sealed class Email : ValueObject
{
    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    /// <summary>Crea un email normalizado, o <c>null</c> si <paramref name="raw"/> es vacío/blanco.
    /// Lanza <see cref="ArgumentException"/> si tiene texto pero no es una dirección con forma mínima
    /// (contiene exactamente un '@' con algo a cada lado).</summary>
    public static Email? Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var valor = raw.Trim().ToLowerInvariant();
        var at = valor.IndexOf('@');
        if (at <= 0 || at == valor.Length - 1 || valor.LastIndexOf('@') != at)
        {
            throw new ArgumentException("El email no es válido.", nameof(raw));
        }

        return new Email(valor);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}