using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Documentos.ValueObjects;

public sealed record DestinoEnvio(
    string Codigo,
    string Nombre,
    string Direccion,
    string CodigoPostal,
    string Ciudad,
    string CodigoPais,
    string? Telefono);