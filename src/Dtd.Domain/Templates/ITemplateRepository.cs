using System;
using System.Collections.Generic;
using System.Text;

namespace Dtd.Domain.Templates
{
    public interface ITemplateRepository
    {
        Task<Template?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default);

        Task<Template?> GetByEmpresaYCodeAsync(string empresa, string code, CancellationToken cancellationToken = default);

        /// <summary>Todas las plantillas de una empresa (vista de gestión: activas e inactivas).</summary>
        Task<IReadOnlyList<Template>> ListarPorEmpresaAsync(string empresa, CancellationToken cancellationToken = default);

        /// <summary>Persiste una nueva plantilla del catálogo.</summary>
        Task AddAsync(Template template, CancellationToken cancellationToken = default);

        /// <summary>Marca la plantilla para actualización (carga con tracking).</summary>
        Task UpdateAsync(Template template, CancellationToken cancellationToken = default);
    }
}