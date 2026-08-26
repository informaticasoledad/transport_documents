using Dtd.Domain.Common;
using Dtd.Domain.Ccs;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Documentos;

/// <summary>
/// CC (destinatario en copia) asignado a un <see cref="DocumentoDigitalTransporte"/>. Es un child entity
/// del agregado: un DDT puede tener **N** CCs (opcionales; no se exige ninguno para enviar). Es un
/// **snapshot** inmutable del catálogo <see cref="Cc"/> en el momento de la asignación, así las ediciones
/// posteriores del catálogo no afectan a documentos en curso. Se crea vía <see cref="CrearDesdeCatalogo"/>;
/// la idempotencia por <see cref="CcCatalogId"/> la enforce el agregado en <c>AsignarCc</c>.
/// <see cref="CcCodigo"/> se guarda sólo como snapshot de display/trazabilidad (no es la clave de
/// idempotencia). Es **email-only**: sin <c>Canal</c>/<c>Movil</c>/<c>TaxId</c> (en Docuten siempre va como
/// <c>signing_role="cc"</c> + <c>channel="email"</c>, dentro del array <c>consignees</c> de Docuten).
/// </summary>
public sealed class CcAsignado : Entity<Guid>
{
    /// <summary><c>Id</c> del CC del catálogo en el momento de la asignación (clave de idempotencia).</summary>
    public Guid CcCatalogId { get; private set; }

    /// <summary>Código del CC en el catálogo (snapshot de display/trazabilidad).</summary>
    public string CcCodigo { get; private set; }

    public string Nombre { get; private set; }
    public Email? Email { get; private set; }
    public string Language { get; private set; }

    /// <summary>Usado por el ORM para materializar la entidad; no para código de aplicación.</summary>
    private CcAsignado()
    {
        CcCodigo = string.Empty;
        Nombre = string.Empty;
        Language = "es";
    }

    private CcAsignado(Guid ccCatalogId, string ccCodigo, string nombre, Email? email, string language)
    {
        // El Id lo fija el dominio (Guid.NewGuid): así es único ya en memoria, antes de persistir, y
        // RemoverCc(id) puede distinguir CCs sin depender de la BD. Para que EF Core no interprete ese
        // Guid no-default como "entidad existente" al añadirlo a la colección de un agregado ya cargado
        // (lo que generaría un UPDATE sobre una fila inexistente → DbUpdateConcurrencyException), la
        // configuración marca la clave como ValueGeneratedNever() (clave generada por el cliente, no por
        // el store). Ver ConductorAsignadoConfiguration para el mismo motivo.
        Id = Guid.NewGuid();
        CcCatalogId = ccCatalogId;
        CcCodigo = ccCodigo;
        Nombre = nombre;
        Email = email;
        Language = language;
    }

    /// <summary>Snapshots un <see cref="Cc"/> del catálogo en un CC asignado al documento.</summary>
    public static CcAsignado CrearDesdeCatalogo(Cc cc)
    {
        ArgumentNullException.ThrowIfNull(cc);

        return new CcAsignado(cc.Id, cc.Codigo, cc.Nombre, cc.Email, cc.Language);
    }
}
