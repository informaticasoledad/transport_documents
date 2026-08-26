namespace Dtd.Domain.Almacenes;

public sealed class AlmacenAgencia
{
    public Guid AlmacenId { get; private set; }
    public Guid AgenciaId { get; private set; }
    public Guid? AgenciaBaseId { get; private set; }
    public Guid? TemplateId { get; private set; }


    private AlmacenAgencia()
    {
    }

    private AlmacenAgencia(Guid almacenId, Guid agenciaId, Guid? agenciaBaseId)
    {
        if (almacenId == Guid.Empty)
        {
            throw new ArgumentException("El almacen es obligatorio.", nameof(almacenId));
        }

        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException("La agencia es obligatoria.", nameof(agenciaId));
        }

        AlmacenId = almacenId;
        AgenciaId = agenciaId;
        AgenciaBaseId = agenciaBaseId;
    }

    public static AlmacenAgencia Crear(Guid almacenId, Guid agenciaId, Guid? agenciaBaseId = null)
    {
        return new AlmacenAgencia(almacenId, agenciaId, agenciaBaseId);
    }

    public void ConfigurarAgenciaBase(Guid? agenciaBaseId)
    {
        AgenciaBaseId = agenciaBaseId == Guid.Empty ? null : agenciaBaseId;
    }

    public void AsignarTemplate(Guid? templateId)
    {
        TemplateId = templateId == Guid.Empty ? null : templateId;
    }
}
