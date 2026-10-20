using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;

namespace Dtd.Domain.Documentos;

public sealed class Envio : Entity<Guid>
{
    public int Orden { get; private set; }

    public string Referencia { get; private set; }

    public int Bultos { get; private set; }

    public decimal PesoTotal { get; private set; }

    public DestinoEnvio? Destino { get; private set; }

    public string? PlataformaEnvioId { get; private set; }

    public string? PlataformaEnvioEstado { get; private set; }

    public int NumeroReenviosNotificacion { get; private set; }

    public DateTimeOffset? FechaUltimoReenvioNotificacion { get; private set; }

    private Envio()
    {
        Referencia = string.Empty;
    }

    private Envio(
        int orden,
        string referencia,
        int bultos,
        decimal pesoTotal,
        DestinoEnvio? destino)
    {
        if (orden < 1)
        {
            throw new ArgumentException(
                "El orden del envío debe ser mayor o igual que 1.",
                nameof(orden));
        }

        if (string.IsNullOrWhiteSpace(referencia))
        {
            throw new ArgumentException(
                "La referencia del envío es obligatoria.",
                nameof(referencia));
        }

        if (bultos < 0)
        {
            throw new ArgumentException(
                "El número de bultos no puede ser negativo.",
                nameof(bultos));
        }

        Id = Guid.NewGuid();

        Orden = orden;
        Referencia = referencia.Trim();
        Bultos = bultos;
        PesoTotal = pesoTotal;
        Destino = destino;
    }

    public static Envio Crear(
        int orden,
        string referencia,
        int bultos,
        decimal pesoTotal,
        DestinoEnvio? destino)
    {
        return new Envio(
            orden,
            referencia,
            bultos,
            pesoTotal,
            destino);
    }

    public void AsignarDestino(
        DestinoEnvio destino)
    {
        ArgumentNullException.ThrowIfNull(destino);

        Destino = destino;
    }

    public bool TieneDestinoValido => Destino is not null;

    public void ConfirmarEnvioPlataforma(
        string? shipmentId,
        string? estado)
    {
        PlataformaEnvioId = NormalizarOpcional(shipmentId);
        PlataformaEnvioEstado = NormalizarOpcional(estado);
    }

    public ResultadoCallbackEnvio RegistrarCallbackDocuten(
    string? shipmentId,
    string? estadoDocuten)
    {
        var modificado = false;

        if (!string.IsNullOrWhiteSpace(shipmentId))
        {
            shipmentId = shipmentId.Trim();

            if (!string.IsNullOrWhiteSpace(PlataformaEnvioId) &&
                !string.Equals(
                    PlataformaEnvioId,
                    shipmentId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoCallbackEnvio.IdentificadorIncompatible;
            }
        }

        if (string.IsNullOrWhiteSpace(estadoDocuten))
        {
            return ResultadoCallbackEnvio.EstadoNoReconocido;
        }

        var nuevoEstado = estadoDocuten.Trim();

        // Para callbacks SIGNATURE admitimos estos estados.
        var nuevoOrden = ObtenerOrdenEstado(nuevoEstado);

        if (nuevoOrden < 0)
        {
            return ResultadoCallbackEnvio.EstadoNoReconocido;
        }

        var ordenActual = ObtenerOrdenEstado(PlataformaEnvioEstado);

        if (string.Equals(
            PlataformaEnvioEstado,
            nuevoEstado,
            StringComparison.OrdinalIgnoreCase))
        {
            return ResultadoCallbackEnvio.YaAplicado;
        }

        if (EsEstadoFinal(PlataformaEnvioEstado))
        {
            return ResultadoCallbackEnvio.Obsoleto;
        }

        if (ordenActual >= 0 && nuevoOrden < ordenActual)
        {
            return ResultadoCallbackEnvio.Obsoleto;
        }

        if (!string.IsNullOrWhiteSpace(shipmentId) &&
            string.IsNullOrWhiteSpace(PlataformaEnvioId))
        {
            PlataformaEnvioId = shipmentId;
            modificado = true;
        }

        PlataformaEnvioEstado = nuevoEstado;

        return ResultadoCallbackEnvio.Aplicado;
    }
    private static bool EsEstadoFinal(string? estado) =>
        string.Equals(estado, EstadoDocuten.Delivered,
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(estado, EstadoDocuten.Completed,
            StringComparison.OrdinalIgnoreCase);

    private static int ObtenerOrdenEstado(string? estado)
    {
        if (string.Equals(estado, EstadoDocuten.Created,
            StringComparison.OrdinalIgnoreCase))
            return 0;

        if (string.Equals(estado, EstadoDocuten.ReadyForPickup,
            StringComparison.OrdinalIgnoreCase))
            return 1;

        if (string.Equals(estado, EstadoDocuten.PendingDelivery,
            StringComparison.OrdinalIgnoreCase))
            return 2;

        if (EsEstadoFinal(estado))
            return 3;

        // Los estados desconocidos no deben tratarse como progreso.
        return -1;
    }
    public void ActualizarTotales(
        int bultos,
        decimal pesoTotal)
    {
        Bultos = bultos;
        PesoTotal = pesoTotal;
    }

    internal ErrorOr<Success> ValidarPuedeReenviarNotificacion(
        int maximoReenvios)
    {
        if (string.IsNullOrWhiteSpace(PlataformaEnvioId))
        {
            return Error.Conflict(
                "Documento.Envio.SinIdPlataforma",
                "El envío no dispone de identificador en Docuten.");
        }

        if (NumeroReenviosNotificacion >= maximoReenvios)
        {
            return Error.Conflict(
                "Documento.Envio.LimiteReenviosNotificacion",
                $"Se ha alcanzado el límite de {maximoReenvios} reenvíos de notificación.");
        }

        return Result.Success;
    }

    internal void RegistrarReenvioNotificacion(
        DateTimeOffset fecha)
    {
        NumeroReenviosNotificacion++;
        FechaUltimoReenvioNotificacion = fecha;
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
}