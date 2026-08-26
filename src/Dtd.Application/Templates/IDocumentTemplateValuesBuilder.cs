using Dtd.Application.GatewayContracts;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Templates
{
    public interface IDocumentTemplateValuesBuilder
    {
        string DocumentType { get; }

        Dictionary<string, string> Build(
            DocumentoDigitalTransporte documento,
            Envio envio,
            EmpresaConfig empresa,
            Almacen almacen,
            Agencia agencia);
    }
}