using Dtd.Application.GatewayContracts;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using System.Globalization;

namespace Dtd.Application.Templates;

public sealed class DecaTemplateValuesBuilder : IDocumentTemplateValuesBuilder
{
    private readonly DocutenMappingOptions _options;

    public DecaTemplateValuesBuilder(DocutenMappingOptions options)
    {
        _options = options;
    }

    public string DocumentType => "transport_control_document";

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

        return new Dictionary<string, string>
        {
            ["Nombre_NIF_Domicilio_CargadorContractual"] =
                BuildCargadorContractual(empresa, almacen),

            ["Observaciones_cargador"] = string.Empty,

            ["Nombre_NIF_Domicilio_TransportistaEfectivo"] =
                BuildTransportistaEfectivo(agencia),

            ["AutorizaciónCirculacion"] = string.Empty,

            ["LugarOrigen"] =
                BuildLugarOrigen(documento),

            ["LugarDestino"] =
                BuildLugarDestino(destino),

            ["NaturalezaMercancia"] =
                _options.DefaultGoodsDescription,

            ["PesoMercancía"] =
                $"{envio.PesoTotal.ToString("0.##", CultureInfo.InvariantCulture)} kg",

            ["FechaTransporte"] =
                BuildFechaTransporte(documento, envio),

            ["MatriculaTractora"] =
                BuildMatriculaTractora(documento),

            ["MatriculaRemolque"] = documento.MatriculaRemolque ?? string.Empty,

            ["Observaciones"] = string.Empty,

            // Información no obligatoria: destinatario / consignatario
            ["Nombre_Destinatario"] =
                destino.Nombre,

            ["Domicilio_Destinatario"] =
                BuildDomicilioDestinatario(destino)
        };
    }

    private static string BuildCargadorContractual(
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

    private static string BuildLugarOrigen(
        DocumentoDigitalTransporte documento)
    {
        return JoinLines(
            documento.Origen.AddressStreet,
            $"{documento.Origen.Zipcode} {documento.Origen.City}",
            documento.Origen.ProvinceName,
            documento.Origen.CountryIsoCode);
    }

    private static string BuildLugarDestino(
        DestinoEnvio destino)
    {
        return JoinLines(
            destino.Direccion,
            $"{destino.CodigoPostal} {destino.Ciudad}",
            destino.CodigoPais);
    }

    private static string BuildDomicilioDestinatario(
        DestinoEnvio destino)
    {
        return JoinLines(
            destino.Direccion,
            $"{destino.CodigoPostal} {destino.Ciudad}",
            destino.CodigoPais);
    }

    private static string BuildFechaTransporte(
        DocumentoDigitalTransporte documento,
        Envio envio)
    {
        var expediciones = documento.Expediciones
            .Where(x => x.EnvioId == envio.Id)
            .OrderBy(x => x.Fecha)
            .ToList();

        if (expediciones.Count == 0)
        {
            return documento.FechaGeneracion.ToString("dd/MM/yyyy");
        }

        return expediciones[0].Fecha.ToString("dd/MM/yyyy");
    }

    private static string BuildMatriculaTractora(
        DocumentoDigitalTransporte documento)
    {
        return documento.Conductores
            .Select(x => x.LicensePlate)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?.Trim()
            ?? string.Empty;
    }

    private static string JoinLines(params string?[] values)
    {
        return string.Join(
            Environment.NewLine,
            values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim()));
    }

    private static string BuildTransportistaEfectivo(
    Agencia agencia)
    {
        return JoinLines(
            agencia.Nombre,
            agencia.IdentificadorFiscal);
    }
}