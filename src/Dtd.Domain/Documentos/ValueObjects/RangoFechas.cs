using Dtd.Domain.Common;

namespace Dtd.Domain.Documentos.ValueObjects;

/// <summary>
/// Inclusive date range used to select expeditions not yet included in any document.
/// </summary>
public sealed class RangoFechas : ValueObject
{
    public DateOnly FechaDesde { get; }
    public DateOnly FechaHasta { get; }

    private RangoFechas(DateOnly fechaDesde, DateOnly fechaHasta)
    {
        FechaDesde = fechaDesde;
        FechaHasta = fechaHasta;
    }

    public static RangoFechas Create(DateOnly fechaDesde, DateOnly fechaHasta)
    {
        if (fechaDesde > fechaHasta)
        {
            throw new ArgumentException(
                "La fecha 'desde' no puede ser posterior a la fecha 'hasta'.",
                nameof(fechaDesde));
        }

        return new RangoFechas(fechaDesde, fechaHasta);
    }

    public bool Contiene(DateOnly fecha) => fecha >= FechaDesde && fecha <= FechaHasta;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FechaDesde;
        yield return FechaHasta;
    }
}