using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.GatewayContracts
{
    public sealed record DocutenPartyResult(
        string PartyId,
        string PartyType,
        string SigningRole,
        int SignOrder,
        string Status,
        DateTimeOffset? NotificationDate);
}