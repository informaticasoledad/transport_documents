using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Documentos;

/// <summary>
/// Conductor asignado a un <see cref="DocumentoDigitalTransporte"/> (driver del lote Docuten). Es un
/// child entity del agregado (como <see cref="Expedicion"/>): un DDT puede tener N conductores
/// (mínimo 1 para enviar). Es un **snapshot** inmutable del catálogo <see cref="Conductor"/> en el
/// momento de la asignación, así las ediciones posteriores del catálogo no afectan a documentos en
/// curso. Se crea vía <see cref="CrearDesdeCatalogo"/>; la idempotencia por <see cref="ConductorCatalogId"/>
/// la enforce el agregado en <c>AsignarConductor</c>. <see cref="ConductorCodigo"/> se guarda sólo
/// como snapshot de display/trazabilidad (ya no es la clave de idempotencia).
/// </summary>
public sealed class ConductorAsignado : Entity<Guid>
{
    /// <summary><c>Id</c> del conductor del catálogo en el momento de la asignación (clave de idempotencia).</summary>
    public Guid ConductorCatalogId { get; private set; }

    /// <summary>Código del conductor en el catálogo (snapshot de display/trazabilidad).</summary>
    public string ConductorCodigo { get; private set; }

    public string Nombre { get; private set; }
    public string? TaxId { get; private set; }
    public string? LicensePlate { get; private set; }
    public Movil? Movil { get; private set; }
    public Email? Email { get; private set; }
    public Canal Canal { get; private set; }
    public string Language { get; private set; }

    /// <summary>Usado por el ORM para materializar la entidad; no para código de aplicación.</summary>
    private ConductorAsignado()
    {
        ConductorCodigo = string.Empty;
        Nombre = string.Empty;
        Canal = null!;
        Language = "es";
    }

    private ConductorAsignado(
        Guid conductorCatalogId, string conductorCodigo, string nombre, string? taxId, string? licensePlate,
        Movil? movil, Email? email, Canal canal, string language)
    {
        // El Id lo fija el dominio (Guid.NewGuid): así es único ya en memoria, antes de persistir, y
        // RemoverConductor(id) puede distinguir conductores sin depender de la BD. Para que EF Core no
        // interprete ese Guid no-default como "entidad existente" al añadirlo a la colección de un
        // agregado ya cargado (lo que generaría un UPDATE sobre una fila inexistente →
        // DbUpdateConcurrencyException), la configuración marca la clave como ValueGeneratedNever()
        // (clave generada por el cliente, no por el store). A diferencia de DocumentoEvento (append-only,
        // sin remove por Id), este sí necesita el Id único desde la creación.
        Id = Guid.NewGuid();
        ConductorCatalogId = conductorCatalogId;
        ConductorCodigo = conductorCodigo;
        Nombre = nombre;
        TaxId = taxId;
        LicensePlate = licensePlate;
        Movil = movil;
        Email = email;
        Canal = canal;
        Language = language;
    }

    /// <summary>Snapshots un <see cref="Conductor"/> del catálogo en un conductor asignado al documento.</summary>
    public static ConductorAsignado CrearDesdeCatalogo(Conductor conductor)
    {
        ArgumentNullException.ThrowIfNull(conductor);

        return new ConductorAsignado(
            conductor.Id,
            conductor.Codigo,
            conductor.Nombre,
            conductor.TaxId,
            conductor.LicensePlate,
            conductor.Movil,
            conductor.Email,
            conductor.Canal,
            conductor.Language);
    }

    /// <summary><c>true</c> si el contacto es coherente con el canal (email→Email; sms/whatsapp→Movil).</summary>
    public bool TieneCanalValido =>
        Canal.RequiereEmail ? Email is not null
        : Canal.RequiereMovil ? Movil is not null
        : false;
}