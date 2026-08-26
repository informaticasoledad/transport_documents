using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// A transportista's mobile phone number. Required by Docuten to notify the carrier.
/// Normalised to digits so the default lookup and the ERP value can be compared reliably.
/// </summary>
public sealed class Movil : ValueObject
{
    public string Valor { get; }

    private Movil(string valor) => Valor = valor;

    public static Movil? Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length is < 6 or > 15)
        {
            throw new ArgumentException("El número de móvil no es válido.", nameof(raw));
        }

        return new Movil(digits);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}