namespace Dtd.Domain.Almacenes;

public sealed class AlmacenAgenciaBaseDefecto
{
    public Guid AlmacenId { get; private set; }
    public Guid AgenciaId { get; private set; }
    public Guid AgenciaBaseId { get; private set; }

    private AlmacenAgenciaBaseDefecto()
    {
    }

    private AlmacenAgenciaBaseDefecto(Guid almacenId, Guid agenciaId, Guid agenciaBaseId)
    {
        AlmacenId = almacenId;
        AgenciaId = agenciaId;
        AgenciaBaseId = agenciaBaseId;
    }

    public static AlmacenAgenciaBaseDefecto Crear(Guid almacenId, Guid agenciaId, Guid agenciaBaseId)
    {
        if (almacenId == Guid.Empty)
        {
            throw new ArgumentException("El almacen es obligatorio.", nameof(almacenId));
        }

        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException("La agencia es obligatoria.", nameof(agenciaId));
        }

        if (agenciaBaseId == Guid.Empty)
        {
            throw new ArgumentException("El agenciaBase es obligatorio.", nameof(agenciaBaseId));
        }

        return new AlmacenAgenciaBaseDefecto(almacenId, agenciaId, agenciaBaseId);
    }
}
