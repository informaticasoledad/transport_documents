using Dtd.Application.GatewayContracts;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Domain.Empresas;
using Dtd.Domain.Templates;
using System.Globalization;

namespace Dtd.Application.Mapping;

public static class DocumentoToDocutenMapper
{
    public static async Task<DocutenLoteDto> ToDocutenLoteDto(
        this DocumentoDigitalTransporte documento,
        EmpresaConfig empresa,
        Almacen almacen,
        Agencia agencia,
        Template template,
        IReadOnlyDictionary<Guid, byte[]> pdfsPorEnvio,
        DocutenMappingOptions options,
        IDocutenDocumentoProvider documentoProvider,
        CancellationToken cancellationToken)
    {
        var language = string.IsNullOrWhiteSpace(template.Language)
            ? string.IsNullOrWhiteSpace(options.DefaultLanguage)
                ? "es"
                : options.DefaultLanguage
            : template.Language;

        var callbackUrl = string.IsNullOrWhiteSpace(options.CallbackUrl)
            ? null
            : options.CallbackUrl.Trim();

        var consignor = BuildConsignor(
            documento,
            empresa,
            almacen,
            language);

        var drivers = BuildDrivers(documento);

        var shipments = new List<DocutenShipmentDto>();

        foreach (var envio in documento.Envios.OrderBy(e => e.Orden))
        {
            var consignees = BuildConsignees(
                envio,
                documento.Ccs,
                drivers,
                language);

            var parties = new List<DocutenPartyDto>
            {
                consignor
            };

            parties.AddRange(drivers);
            parties.AddRange(consignees);

            var documentoDto = await documentoProvider.ObtenerDocumentoAsync(
                documento,
                envio,
                empresa,
                almacen,
                agencia,
                template,
                parties,
                cancellationToken);

            if (!pdfsPorEnvio.TryGetValue(envio.Id, out var pdfEnvio))
            {
                throw new InvalidOperationException(
                    $"No se ha proporcionado el PDF para el envío '{envio.Referencia}'.");
            }

            var documentoAlbaranes = new DocutenDocumentoDto
            {
                DocumentType = "delivery_note",
                DocumentName = $"albaranes-{envio.Referencia}.pdf",
                ExternalId = $"{envio.Referencia}-albaranes",
                Signable = false,
                Content = Convert.ToBase64String(pdfEnvio),

                Signers = documentoDto.Signers?
                    .Select(s => new DocutenSignerDto
                    {
                        Order = s.Order
                    })
                    .ToList()
            };

            shipments.Add(new DocutenShipmentDto
            {
                ShipmentReference = envio.Referencia,
                ShipmentName = envio.Referencia,
                CallbackUrl = callbackUrl,
                Language = language,

                Origin = new DocutenOrigenDto
                {
                    Address = JoinAddress(
                        documento.Origen.AddressStreet,
                        documento.Origen.AddressName),
                    PostCode = documento.Origen.Zipcode,
                    City = documento.Origen.City,
                    CountryCode = documento.Origen.CountryIsoCode
                },

                Destination = BuildDestination(envio),

                Parties = new DocutenPartiesDto
                {
                    Consignors = [consignor],
                    Drivers = drivers,
                    Consignees = consignees
                },

                Goods =
                [
                    new DocutenGoodsDto
                    {
                        Description = $"{envio.Bultos} bultos",
                        //CargoType = "paletizado",
                        GrossMass =
                            $"{envio.PesoTotal.ToString("0.##", CultureInfo.InvariantCulture)} kg",
                        DangerousGoods = false
                    }
                ],

                Documents =
                [
                    documentoDto,
                    documentoAlbaranes
                ],

                Metadata =
                [
                    new DocutenMetadataDto
                    {
                        Name = "empresa",
                        Value = documento.Empresa
                    },
                    new DocutenMetadataDto
                    {
                        Name = "almacen",
                        Value = almacen.Codigo
                    },
                    new DocutenMetadataDto
                    {
                        Name = "agencia",
                        Value = agencia.Codigo
                    },
                    new DocutenMetadataDto
                    {
                        Name = "envio",
                        Value = envio.Orden.ToString()
                    },
                    new DocutenMetadataDto
                    {
                        Name = "template",
                        Value = template.Code
                    }
                ]
            });
        }

        return new DocutenLoteDto
        {
            LotReference = documento.Id.ToString(),
            LotName = $"DDT {documento.Empresa}/{almacen.Codigo}/{agencia.Codigo}",
            CallbackUrl = callbackUrl,
            Shipments = shipments
        };
    }

    private static DocutenPartyDto BuildConsignor(
        DocumentoDigitalTransporte documento,
        EmpresaConfig empresa,
        Almacen almacen,
        string language)
    {
        var consignorName = string.IsNullOrWhiteSpace(empresa.Nombre)
            ? $"Empresa {documento.Empresa}"
            : empresa.Nombre;

        return new DocutenPartyDto
        {
            Name = consignorName,
            TaxId = empresa.TaxId,
            Order = 1,
            SigningRole = "signer",
            SignatureType = almacen.TipoFirmaConsignor,
            Channel = almacen.Email is not null
                ? "email"
                : almacen.Telefono is not null
                    ? "sms"
                    : null,
            Email = almacen.Email?.Valor,
            Mobile = FormatE164(almacen.Telefono),
            Language = language
        };
    }

    private static List<DocutenPartyDto> BuildDrivers(
        DocumentoDigitalTransporte documento)
    {
        return documento.Conductores
            .Select((c, i) => new DocutenPartyDto
            {
                Name = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Mobile = FormatE164(c.Movil?.Valor),
                Language = c.Language,
                Order = 2 + i,
                SigningRole = "signer",
                SignatureType = "biometric"
            })
            .ToList();
    }

    private static DocutenDestinoDto BuildDestination(
        Envio envio)
    {
        var destino = GetDestino(envio);

        return new DocutenDestinoDto
        {
            Address = destino.Direccion,
            PostCode = destino.CodigoPostal,
            City = destino.Ciudad,
            CountryCode = destino.CodigoPais
        };
    }

    private static IReadOnlyList<DocutenPartyDto> BuildConsignees(
        Envio envio,
        IReadOnlyCollection<CcAsignado> ccs,
        IReadOnlyList<DocutenPartyDto> drivers,
        string language)
    {
        var destino = GetDestino(envio);

        var ultimoConductor = drivers
            .OrderBy(d => d.Order)
            .Last();

        if (string.IsNullOrWhiteSpace(ultimoConductor.Mobile))
        {
            throw new InvalidOperationException(
                $"El último conductor del envío '{envio.Referencia}' no tiene móvil.");
        }

        var entrega = new DocutenPartyDto
        {
            Name = destino.Nombre,
            Order = 2 + drivers.Count,
            SigningRole = "signer",
            SignatureType = "biometric",
            Channel = "sms",
            Mobile = ultimoConductor.Mobile,
            Language = language,
            Address = destino.Direccion,
            PostCode = destino.CodigoPostal,
            City = destino.Ciudad,
            CountryCode = destino.CodigoPais
        };

        var copias = ccs
            .Select((c, i) => new DocutenPartyDto
            {
                Name = c.Nombre,
                Order = 3 + drivers.Count + i,
                SigningRole = "cc",
                Channel = "email",
                Email = c.Email?.Valor,
                Language = c.Language
            });

        return [entrega, .. copias];
    }

    private static DestinoEnvio GetDestino(
        Envio envio)
    {
        return envio.Destino
            ?? throw new InvalidOperationException(
                $"El envio '{envio.Referencia}' no tiene destino.");
    }

    private static string? FormatE164(
        string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var digits = new string(
            raw.Where(char.IsDigit).ToArray());

        return digits.Length == 0
            ? null
            : "+" + digits;
    }

    private static string JoinAddress(
        string? street,
        string? name)
    {
        var parts = new[] { street, name }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());

        var joined = string.Join(", ", parts);

        return string.IsNullOrWhiteSpace(joined)
            ? "-"
            : joined;
    }
}