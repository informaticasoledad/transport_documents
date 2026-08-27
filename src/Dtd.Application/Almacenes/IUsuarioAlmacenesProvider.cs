using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Application.Almacenes
{
    public interface IUsuarioAlmacenesProvider
    {
        Task<IReadOnlyCollection<string>> ObtenerAlmacenesPermitidosAsync(
            string usuario,
            string empresa,
            CancellationToken cancellationToken = default);
    }
}