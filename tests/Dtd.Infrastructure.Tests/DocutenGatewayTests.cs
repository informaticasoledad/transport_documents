using System.Net;
using System.Net.Http;
using System.Text;
using Dtd.Application.GatewayContracts;
using Dtd.Domain.Documentos;
using Dtd.Infrastructure.Configuration;
using Dtd.Infrastructure.Gateways;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Dtd.Infrastructure.Tests;

public class DocutenGatewayTests
{
    /// <summary>Docuten puede devolver 2xx (incluso con lot_id) y a la vez un sobre `error_code` cuando
    /// el lote tiene errores de validación: el lote no se crea realmente. El gateway debe tratarlo como
    /// fallo (lanzar) para que el handler NO marque el documento Enviando sobre un envío rechazado.</summary>
    [Fact]
    public async Task EnviarAsync_lanza_cuando_la_respuesta_es_2xx_con_error_code()
    {
        var body = """{"error_code":"VALIDATION_ERROR","error_message":"The request contains validation errors","details":["validateLot.lot.shipments[0].goods[0].dangerousGoods must not be null"]}""";
        var gateway = CreateGateway(new StubHandler(HttpStatusCode.OK, body));

        var act = async () => await gateway.EnviarAsync(Lote(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DocutenGatewayException>();
        ex.Which.StatusCode.Should().Be(200);
        ex.Which.Body.Should().Contain("VALIDATION_ERROR");
    }

    [Fact]
    public async Task EnviarAsync_devuelve_lot_id_cuando_la_respuesta_es_2xx_sin_error_code()
    {
        var body = """{"lot_id":"LOT-123","status":"pending"}""";
        var gateway = CreateGateway(new StubHandler(HttpStatusCode.Created, body));

        var result = await gateway.EnviarAsync(Lote(), CancellationToken.None);

        result.LotId.Should().Be("LOT-123");
        result.Estado.Should().Be(EstadoDocuten.Pending);
    }

    [Fact]
    public async Task EnviarAsync_lanza_con_el_body_completo_en_error_4xx()
    {
        var body = """{"error_code":"VALIDATION_ERROR","error_message":"bad"}""";
        var gateway = CreateGateway(new StubHandler(HttpStatusCode.BadRequest, body));

        var act = async () => await gateway.EnviarAsync(Lote(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DocutenGatewayException>();
        ex.Which.StatusCode.Should().Be(400);
        ex.Which.Body.Should().Be(body);
    }

    private static DocutenGateway CreateGateway(HttpMessageHandler handler)
        => new(new HttpClient(handler),
            Options.Create(new DocutenOptions { BaseAddress = "http://localhost/", TokenId = "test-key" }),
            NullLogger<DocutenGateway>.Instance);

    private static DocutenLoteDto Lote() => new()
    {
        LotReference = "ref-1",
        LotName = "Lote 1",
        Shipments =
        [
            new DocutenShipmentDto
            {
                ShipmentReference = "s-1",
                ShipmentName = "S1",
                Language = "es",
                Origin = new DocutenOrigenDto { Address = "-" },
                Destination = new DocutenDestinoDto { Address = "-" },
                Parties = new DocutenPartiesDto
                {
                    Consignors = [new() { Name = "Consignor", Order = 1, SigningRole = "signer", SignatureType = "automated" }],
                    Drivers = [],
                    Consignees = []
                },
                Goods = [new() { Description = "1 bulto" }],
                Metadata = []
            }
        ]
    };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public StubHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
    }
}