using Dtd.Domain.Templates;

namespace Dtd.Domain.Almacenes;

public sealed class AlmacenAgencia
{
    public Guid AlmacenId { get; private set; }
    public Guid AgenciaId { get; private set; }
    public Guid? AgenciaBaseId { get; private set; }
    public Guid TemplateId { get; private set; }

    public Template Template { get; private set; } = null!;

    public ICollection<AlmacenAgenciaConductorDefecto> ConductoresDefecto { get; set; }
    = new List<AlmacenAgenciaConductorDefecto>();

    private AlmacenAgencia()
    {
    }

    private AlmacenAgencia(
        Guid almacenId,
        Guid agenciaId,
        Guid templateId,
        Guid? agenciaBaseId)
    {
        if (almacenId == Guid.Empty)
        {
            throw new ArgumentException(
                "El almacén es obligatorio.",
                nameof(almacenId));
        }

        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La agencia es obligatoria.",
                nameof(agenciaId));
        }

        if (templateId == Guid.Empty)
        {
            throw new ArgumentException(
                "El template es obligatorio.",
                nameof(templateId));
        }

        AlmacenId = almacenId;
        AgenciaId = agenciaId;
        TemplateId = templateId;
        AgenciaBaseId =
            agenciaBaseId == Guid.Empty
                ? null
                : agenciaBaseId;
    }

    public static AlmacenAgencia Crear(
        Guid almacenId,
        Guid agenciaId,
        Guid templateId,
        Guid? agenciaBaseId = null)
    {
        return new AlmacenAgencia(
            almacenId,
            agenciaId,
            templateId,
            agenciaBaseId);
    }

    public void ConfigurarAgenciaBase(
        Guid? agenciaBaseId)
    {
        AgenciaBaseId =
            agenciaBaseId == Guid.Empty
                ? null
                : agenciaBaseId;
    }

    public void CambiarTemplate(
        Guid templateId)
    {
        if (templateId == Guid.Empty)
        {
            throw new ArgumentException(
                "El template es obligatorio.",
                nameof(templateId));
        }

        TemplateId = templateId;
    }
}