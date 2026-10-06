using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.GatewayContracts
{
    public sealed record DocutenShipmentResult(
        string ShipmentId,
        string ShipmentStatus,
        int SignaturesDone,
        int SignaturesTotal,
        IReadOnlyList<DocutenPartyResult> Parties);
}