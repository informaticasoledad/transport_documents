using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Almacenes.ListarAlmacenes
{
    public sealed class AlmacenesPaginadosDto
    {
        public IReadOnlyList<AlmacenDto> Items { get; init; } = [];
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}