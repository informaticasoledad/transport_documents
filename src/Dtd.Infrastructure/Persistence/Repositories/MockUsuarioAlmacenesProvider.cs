using Dtd.Application.Almacenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Infrastructure.Persistence.Repositories
{
    internal sealed class MockUsuarioAlmacenesProvider
        : IUsuarioAlmacenesProvider
    {
        public Task<IReadOnlyCollection<string>> ObtenerAlmacenesPermitidosAsync(
            string usuario,
            string empresa,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<string> almacenes =
            [
                "21",
                "54",
                "78"
            ];

            return Task.FromResult(almacenes);
        }
    }
}