using Dtd.Domain.Ccs;

namespace Dtd.Domain.Almacenes;

public sealed class AlmacenAgenciaCc
{
    public Guid AlmacenId { get; private set; }
    public Guid AgenciaId { get; private set; }
    public Guid CcId { get; private set; }
    public bool PorDefecto { get; private set; }

    private AlmacenAgenciaCc()
    {
    }

    private AlmacenAgenciaCc(Guid almacenId, Guid agenciaId, Guid ccId, bool porDefecto)
    {
        AlmacenId = almacenId;
        AgenciaId = agenciaId;
        CcId = ccId;
        PorDefecto = porDefecto;
    }

    public static AlmacenAgenciaCc Crear(Guid almacenId, Guid agenciaId, Guid ccId, bool porDefecto = false)
    {
        if (almacenId == Guid.Empty)
        {
            throw new ArgumentException("El almacen es obligatorio.", nameof(almacenId));
        }

        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException("La agencia es obligatoria.", nameof(agenciaId));
        }

        if (ccId == Guid.Empty)
        {
            throw new ArgumentException("El CC es obligatorio.", nameof(ccId));
        }

        return new AlmacenAgenciaCc(almacenId, agenciaId, ccId, porDefecto);
    }

    public void ConfigurarPorDefecto(bool porDefecto) => PorDefecto = porDefecto;
}
