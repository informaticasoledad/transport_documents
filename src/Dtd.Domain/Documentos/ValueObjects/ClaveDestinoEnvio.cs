namespace Dtd.Domain.Documentos.ValueObjects;

public static class ClaveDestinoEnvio
{
    public static string ParaAlmacen(string codigo) =>
        $"ALMACEN:{codigo.Trim()}";

    public static string ParaCliente(
        string? cliente,
        DestinoExpedicion destino) =>
        string.Join(
            "|",
            "CLIENTE",
            cliente?.Trim() ?? string.Empty,
            destino.AddressName?.Trim() ?? string.Empty,
            destino.AddressStreet?.Trim() ?? string.Empty,
            destino.CodigoPostal?.Trim() ?? string.Empty,
            destino.Municipio?.Trim() ?? string.Empty,
            destino.Pais?.Trim() ?? string.Empty);
}