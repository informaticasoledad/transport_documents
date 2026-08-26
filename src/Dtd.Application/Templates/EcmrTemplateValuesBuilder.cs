using Dtd.Application.GatewayContracts;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;

namespace Dtd.Application.Templates;

public sealed class EcmrTemplateValuesBuilder : IDocumentTemplateValuesBuilder
{
    public string DocumentType => "ecmr";

    public Dictionary<string, string> Build(
        DocumentoDigitalTransporte documento,
        Envio envio,
        EmpresaConfig empresa,
        Almacen almacen,
        Agencia agencia)
    {
        var destino = envio.Destino
            ?? throw new InvalidOperationException(
                $"El envío '{envio.Referencia}' no tiene destino.");

        var values = new Dictionary<string, string>
        {
            ["PV"] = documento.Referencia,

            ["Remitente"] = BuildRemitente(
                empresa,
                almacen),

            ["Destinatario"] = BuildDestinatario(
                destino),

            ["Lugar de entrega"] = BuildLugarEntrega(
                destino),

            ["Lugar de carga"] = BuildLugarCarga(
                documento),

            // De momento Agencia solo contiene nombre/código.
            // Faltan NIF y domicilio del transportista.
            ["Porteador"] = agencia.Nombre,

            ["Porteadores sucesivos"] = string.Empty,

            ["Reservas y observaciones porteador"] = string.Empty,

            ["Instrucciones remitente"] = string.Empty,

            ["Total 1"] = string.Empty,

            ["Total 2"] = string.Empty,

            ["Formalizado en"] = BuildFormalizadoEn(
                documento)
        };

        AddMercancias(
            values,
            envio);

        return values;
    }

    private static string BuildRemitente(
        EmpresaConfig empresa,
        Almacen almacen)
    {
        return JoinLines(
            empresa.Nombre,
            empresa.TaxId,
            almacen.Direccion,
            $"{almacen.CodigoPostal} {almacen.Ciudad}",
            almacen.CodigoPaisIso);
    }

    private static string BuildDestinatario(
        DestinoEnvio destino)
    {
        return JoinLines(
            destino.Nombre,
            destino.Direccion,
            $"{destino.CodigoPostal} {destino.Ciudad}",
            destino.CodigoPais);
    }

    private static string BuildLugarEntrega(
        DestinoEnvio destino)
    {
        return JoinLines(
            destino.Direccion,
            $"{destino.CodigoPostal} {destino.Ciudad}",
            destino.CodigoPais);
    }

    private static string BuildLugarCarga(
        DocumentoDigitalTransporte documento)
    {
        return JoinLines(
            documento.Origen.AddressStreet,
            BuildLocalidadOrigen(documento.Origen),
            documento.Origen.CountryName,
            documento.FechaGeneracion.ToString("dd/MM/yyyy"));
    }

    private static string BuildFormalizadoEn(
        DocumentoDigitalTransporte documento)
    {
        var lugar = string.Join(
            ", ",
            new[]
            {
                documento.Origen.City,
                documento.Origen.ProvinceName
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        var fecha = documento.FechaGeneracion.ToString("dd/MM/yyyy");

        return string.IsNullOrWhiteSpace(lugar)
            ? fecha
            : $"{lugar}, a {fecha}";
    }

    private static string BuildLocalidadOrigen(
        OrigenDocumento origen)
    {
        var localidad = string.Join(
            " ",
            new[]
            {
                origen.Zipcode,
                origen.City
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(origen.ProvinceName))
        {
            localidad = string.IsNullOrWhiteSpace(localidad)
                ? origen.ProvinceName
                : $"{localidad} ({origen.ProvinceName})";
        }

        return localidad;
    }

    private static void AddMercancias(
        Dictionary<string, string> values,
        Envio envio)
    {
        // Actualmente el dominio solo dispone del número de bultos.
        // Peso, volumen, embalaje, descripción y código estadístico
        // no están disponibles todavía.

        values["Marcas y numeros"] = string.Empty;
        values["Numero bultos"] = envio.Bultos.ToString();
        values["Embalaje"] = string.Empty;
        values["Mercancia"] = string.Empty;
        values["Stats"] = string.Empty;
        values["Peso bruto"] = string.Empty;
        values["Volumen"] = string.Empty;

        for (var i = 2; i <= 6; i++)
        {
            values[$"Marcas y numeros{i}"] = string.Empty;
            values[$"Numero bultos{i}"] = string.Empty;
            values[$"Embalaje{i}"] = string.Empty;
            values[$"Mercancia{i}"] = string.Empty;
            values[$"Stats{i}"] = string.Empty;
            values[$"Peso bruto{i}"] = string.Empty;
            values[$"Volumen{i}"] = string.Empty;
        }
    }

    private static string JoinLines(params string?[] values)
    {
        return string.Join(
            Environment.NewLine,
            values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim()));
    }
}