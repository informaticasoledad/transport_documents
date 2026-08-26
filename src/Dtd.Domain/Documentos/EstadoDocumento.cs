namespace Dtd.Domain.Documentos;

/// <summary>
/// Pipeline de vida del documento digital de transporte desde la perspectiva de nuestro sistema:
/// Nuevo -> Enviando -> PendienteFirmas -> Finalizado, con estados de salida Anulado, Error y
/// Cancelado. El estado de plataforma se guarda aparte en PlataformaEstado/PlataformaEnvioEstado.
/// </summary>
public enum EstadoDocumento
{
    /// <summary>Generado localmente; la plataforma no ha creado ningun lote.</summary>
    Nuevo = 0,

    /// <summary>La plataforma ha aceptado inicialmente el lote y ha devuelto identificador; falta callback.</summary>
    Enviando = 1,

    /// <summary>La plataforma ha confirmado por callback que el lote/envios existen; esperando firmas.</summary>
    PendienteFirmas = 2,

    /// <summary>Estado de progreso posterior a la firma/recogida, si la plataforma lo diferencia.</summary>
    EnProgreso = 3,

    /// <summary>La plataforma informa que el documento se ha completado/entregado.</summary>
    Finalizado = 4,

    /// <summary>Anulacion forzada desde el front.</summary>
    Anulado = 5,

    /// <summary>Se ha producido un error en la plataforma tras crear el lote.</summary>
    Error = 6,

    /// <summary>La plataforma informa que el lote/shipment se ha cancelado.</summary>
    Cancelado = 7
}
