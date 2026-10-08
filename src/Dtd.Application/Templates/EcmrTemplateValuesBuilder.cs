using Dtd.Application.GatewayContracts;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using System.Globalization;

namespace Dtd.Application.Templates;

public sealed class EcmrTemplateValuesBuilder : IDocumentTemplateValuesBuilder
{
    private const string DefaultPackaging = "colis";

    private readonly DocutenMappingOptions _options;

    public EcmrTemplateValuesBuilder(
        DocutenMappingOptions options)
    {
        _options = options;
    }

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

            ["Porteador"] = BuildPorteador(
                agencia),

            ["Porteadores sucesivos"] =
                string.Empty,

            ["Reservas y observaciones porteador"] =
                BuildReservasPorteador(
                    documento),

            ["Instrucciones remitente"] =
                BuildInstruccionesRemitente(documento),

            ["Total 1"] =
                string.Empty,

            ["Total 2"] =
                string.Empty,

            ["Formalizado en"] = BuildFormalizadoEn(
                documento)
        };

        AddMercancias(
            values,
            envio,
            agencia,
            _options.DefaultGoodsDescription);

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
            BuildCodigoPostalCiudad(
                almacen.CodigoPostal,
                almacen.Ciudad),
            almacen.CodigoPaisIso);
    }

    private static string BuildDestinatario(
        DestinoEnvio destino)
    {
        return JoinLines(
            destino.Nombre,
            destino.Direccion,
            BuildCodigoPostalCiudad(
                destino.CodigoPostal,
                destino.Ciudad),
            destino.CodigoPais);
    }

    private static string BuildLugarEntrega(
        DestinoEnvio destino)
    {
        return JoinLines(
            destino.Direccion,
            BuildCodigoPostalCiudad(
                destino.CodigoPostal,
                destino.Ciudad),
            destino.CodigoPais);
    }

    private static string BuildLugarCarga(
        DocumentoDigitalTransporte documento)
    {
        return JoinLines(
            documento.Origen.AddressStreet,
            BuildLocalidadOrigen(
                documento.Origen),
            documento.Origen.CountryName,
            documento.FechaGeneracion.ToString(
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture));
    }

    private static string BuildPorteador(
        Agencia agencia)
    {
        return JoinLines(
            agencia.Nombre,
            agencia.IdentificadorFiscal,
            agencia.Direccion,
            BuildCodigoPostalCiudad(
                agencia.CodigoPostal,
                agencia.Municipio),
            agencia.CodigoPaisIso);
    }

    private static string BuildReservasPorteador(
        DocumentoDigitalTransporte documento)
    {
        return JoinInline(
            " - ",
            documento.Matricula,
            documento.MatriculaRemolque);
    }

    private static string BuildInstruccionesRemitente(
        DocumentoDigitalTransporte documento)
    {
        if (string.IsNullOrWhiteSpace(documento.Precinto))
        {
            return string.Empty;
        }

        return $"Precinto: {documento.Precinto.Trim()}";
    }

    private static string BuildFormalizadoEn(
        DocumentoDigitalTransporte documento)
    {
        var lugar = JoinInline(
            ", ",
            documento.Origen.City,
            documento.Origen.ProvinceName);

        var fecha = documento.FechaGeneracion.ToString(
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture);

        return string.IsNullOrWhiteSpace(lugar)
            ? fecha
            : $"{lugar}, a {fecha}";
    }

    private static string BuildLocalidadOrigen(
        OrigenDocumento origen)
    {
        var localidad = BuildCodigoPostalCiudad(
            origen.Zipcode,
            origen.City);

        if (!string.IsNullOrWhiteSpace(
                origen.ProvinceName))
        {
            localidad = string.IsNullOrWhiteSpace(
                localidad)
                ? origen.ProvinceName.Trim()
                : $"{localidad} ({origen.ProvinceName.Trim()})";
        }

        return localidad;
    }

    private static void AddMercancias(
        Dictionary<string, string> values,
        Envio envio,
        Agencia agencia,
        string goodsDescription)
    {
        values["Marcas y numeros"] =
            agencia.Codigo;

        values["Numero bultos"] =
            envio.Bultos.ToString(
                CultureInfo.InvariantCulture);

        values["Embalaje"] =
            DefaultPackaging;

        values["Mercancia"] =
            goodsDescription;

        values["Stats"] =
            string.Empty;

        values["Peso bruto"] =
            $"{envio.PesoTotal.ToString(
                "0.##",
                CultureInfo.InvariantCulture)} kg";

        values["Volumen"] =
            string.Empty;

        AddLineasMercanciaVacias(
            values);
    }

    private static void AddLineasMercanciaVacias(
        Dictionary<string, string> values)
    {
        for (var i = 2; i <= 6; i++)
        {
            values[$"Marcas y numeros{i}"] =
                string.Empty;

            values[$"Numero bultos{i}"] =
                string.Empty;

            values[$"Embalaje{i}"] =
                string.Empty;

            values[$"Mercancia{i}"] =
                string.Empty;

            values[$"Stats{i}"] =
                string.Empty;

            values[$"Peso bruto{i}"] =
                string.Empty;

            values[$"Volumen{i}"] =
                string.Empty;
        }
    }

    private static string BuildCodigoPostalCiudad(
        string? codigoPostal,
        string? ciudad)
    {
        return JoinInline(
            " ",
            codigoPostal,
            ciudad);
    }

    private static string JoinLines(
        params string?[] values)
    {
        return string.Join(
            Environment.NewLine,
            values
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x!.Trim()));
    }

    private static string JoinInline(
        string separator,
        params string?[] values)
    {
        return string.Join(
            separator,
            values
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x!.Trim()));
    }


}