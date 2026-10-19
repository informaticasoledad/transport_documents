using Dtd.Domain.Common;
using System.Text.RegularExpressions;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// Número de móvil utilizado para las notificaciones de Docuten.
/// Debe incluir el prefijo internacional.
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

        if (!Regex.IsMatch(digits, @"^[1-9]\d{9,14}$"))
        {
            throw new ArgumentException(
                "El móvil debe incluir el prefijo internacional " +
                "y contener entre 10 y 15 dígitos. " +
                "Ejemplo: 34600123123.",
                nameof(raw));
        }

        return new Movil(digits);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}