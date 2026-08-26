using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Documentos;

/// <summary>
/// A single expedition included in a digital transport document. It is a child entity
/// of the <see cref="DocumentoDigitalTransporte"/> aggregate and is only persisted once
/// it has been linked to a document, which is how "expediciones no incluidas todavía"
/// is tracked (by the absence of the <see cref="ErpId"/> in the store).
/// </summary>
/// <remarks>
/// El transportista NO vive en la expedición: un DDT tiene un único transportista a nivel de
/// documento (Docuten notifica a un carrier por DDT), resuelto desde el default de agencia.
/// Tampoco se persisten peso, importe, observaciones ni manual (no los aporta el ERP); los bultos
/// se derivan del número de líneas de detalle (<c>expeditionDetails.Count</c>) al ingerir.
/// </remarks>
public sealed class Expedicion : Entity<Guid>
{
    /// <summary>Identificador de la expedición en el ERP (clave compuesta, p. ej. "0025776338|26").</summary>
    public string ErpId { get; private set; }

    /// <summary>Nº de documento/albarán de la expedición en el ERP (campo <c>documentNumber</c>).</summary>
    public string? DocumentNumber { get; private set; }

    /// <summary>Código de expedición en el ERP (campo <c>expeditionCode</c>).</summary>
    public string? ExpeditionCode { get; private set; }

    /// <summary>Tipo de expedición: 1 = entrega a cliente, 2 = transfer entre almacenes.</summary>
    public int ExpeditionType { get; private set; }

    /// <summary>Tipo de expedición del ERP: entrega a cliente (albarán).</summary>
    public const int TipoCliente = 1;

    /// <summary>Tipo de expedición del ERP: trasiego / transferencia entre almacenes.</summary>
    public const int TipoTrasiego = 2;

    /// <summary>La empresa a la que pertenece la expedición (columna multiempresa).</summary>
    public string Empresa { get; private set; }

    /// <summary>El almacén de origen por el que se filtró la expedición al pedirla al ERP (warehouseId). FK a <c>almacenes</c>.</summary>
    public Guid AlmacenId { get; private set; }

    /// <summary>La agencia/carrier por el que se rutó la expedición (carrierId en el ERP). FK a <c>agencias</c>.</summary>
    public Guid AgenciaId { get; private set; }

    /// <summary>Fecha de la expedición; usada por el filtro de rango al seleccionar no incluidas.</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>El código de cliente (ERP <c>customerId</c>); nulo en transferencias entre almacenes.</summary>
    public string? Cliente { get; private set; }

    public DestinoExpedicion Destino { get; private set; }

    /// <summary>Nº de bultos, derivado de <c>expeditionDetails.Count</c> al ingerir la expedición.</summary>
    public int Bultos { get; private set; }

    /// <summary>El envío (shipment Docuten) al que pertenece esta expedición tras la agrupación del DDT
    /// (<see cref="DocumentoDigitalTransporte.ConstruirEnvios"/>). <c>null</c> hasta que se construyen los
    /// envíos (documentos preexistentes a la feature pueden quedar a <c>null</c>). FK a la tabla de envíos del documento.</summary>
    public Guid? EnvioId { get; private set; }

    /// <summary>Usado por el ORM para materializar la entidad; no para código de aplicación.</summary>
    private Expedicion()
    {
        ErpId = string.Empty;
        Empresa = string.Empty;
        Destino = null!;
    }

    private Expedicion(
        string erpId,
        string? documentNumber,
        string? expeditionCode,
        int expeditionType,
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        DateOnly fecha,
        string? cliente,
        DestinoExpedicion destino,
        int bultos)
    {
        if (string.IsNullOrWhiteSpace(erpId))
        {
            throw new ArgumentException("El identificador ERP de la expedición es obligatorio.", nameof(erpId));
        }

        if (string.IsNullOrWhiteSpace(empresa))
        {
            throw new ArgumentException("La empresa es obligatoria.", nameof(empresa));
        }

        if (almacenId == Guid.Empty)
        {
            throw new ArgumentException("El almacén es obligatorio.", nameof(almacenId));
        }

        if (agenciaId == Guid.Empty)
        {
            throw new ArgumentException("La agencia es obligatoria.", nameof(agenciaId));
        }

        ErpId = erpId.Trim();
        DocumentNumber = documentNumber;
        ExpeditionCode = expeditionCode;
        ExpeditionType = expeditionType;
        Empresa = empresa.Trim();
        AlmacenId = almacenId;
        AgenciaId = agenciaId;
        Fecha = fecha;
        Cliente = cliente;
        Destino = destino ?? throw new ArgumentNullException(nameof(destino));
        Bultos = bultos;
    }

    /// <summary>Factory used when materialising an expedition coming from the ERP.</summary>
    public static Expedicion CrearDesdeErp(
        string erpId,
        string? documentNumber,
        string? expeditionCode,
        int expeditionType,
        string empresa,
        Guid almacenId,
        Guid agenciaId,
        DateOnly fecha,
        string? cliente,
        DestinoExpedicion destino,
        int bultos) =>
        new(erpId, documentNumber, expeditionCode, expeditionType, empresa, almacenId, agenciaId,
            fecha, cliente, destino, bultos);

    /// <summary>Vincula la expedición a su envío (shipment) tras la agrupación del DDT. Lo invoca
    /// <see cref="DocumentoDigitalTransporte.ConstruirEnvios"/>; no se debe llamar desde código de
    /// aplicación.</summary>
    internal void AsignarAEnvio(Guid envioId)
    {
        if (envioId == Guid.Empty)
        {
            throw new ArgumentException("El id de envío es obligatorio.", nameof(envioId));
        }

        EnvioId = envioId;
    }
}
