using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Domain.Documentos;

public sealed class Envio : Entity<Guid>
{
    public int Orden { get; private set; }

    public string Referencia { get; private set; }

    public int Bultos { get; private set; }

    public DestinoEnvio? Destino { get; private set; }
    public string? PlataformaEnvioId { get; private set; }
    public string? PlataformaEnvioEstado { get; private set; }

    private Envio()
    {
        Referencia = string.Empty;
    }

    private Envio(
        int orden,
        string referencia,
        int bultos,
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
        Destino = destino;
    }

    public static Envio Crear(
        int orden,
        string referencia,
        int bultos,
        DestinoEnvio? destino)
    {
        return new Envio(
            orden,
            referencia,
            bultos,
            destino);
    }

    public void AsignarDestino(
        DestinoEnvio destino)
    {
        ArgumentNullException.ThrowIfNull(destino);

        Destino = destino;
    }

    public bool TieneDestinoValido => Destino is not null;

    public void ConfirmarEnvioPlataforma(string? shipmentId, string? estado)
    {
        PlataformaEnvioId = NormalizarOpcional(shipmentId);
        PlataformaEnvioEstado = NormalizarOpcional(estado);
    }

    public void RegistrarCallbackDocuten(string? shipmentId, string? estadoDocuten)
    {
        if (!string.IsNullOrWhiteSpace(shipmentId))
        {
            if (!string.IsNullOrWhiteSpace(PlataformaEnvioId) &&
                !string.Equals(PlataformaEnvioId, shipmentId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            PlataformaEnvioId = shipmentId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(estadoDocuten))
        {
            PlataformaEnvioEstado = estadoDocuten.Trim();
        }
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
