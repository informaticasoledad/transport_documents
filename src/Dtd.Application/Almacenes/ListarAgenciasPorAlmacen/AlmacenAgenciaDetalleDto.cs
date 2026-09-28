using Dtd.Application.Agencias;
using Dtd.Application.Documentos;
using Dtd.Application.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Almacenes.ListarAgenciasPorAlmacen
{
    public sealed record AlmacenAgenciaDetalleDto(
        Guid Id,
        string Codigo,
        string Nombre,
        bool EntregaEnDestino,
        bool RequierePrecinto,
        TemplateDto? Template,
        AgenciaBaseDto? BaseDefecto,
        IReadOnlyList<ConductorDto> ConductoresDefecto,
        IReadOnlyList<CcDto> CcsDefecto);
}