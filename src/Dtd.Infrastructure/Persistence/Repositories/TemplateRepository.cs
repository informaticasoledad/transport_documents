using Dtd.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Repositories;

internal sealed class TemplateRepository : ITemplateRepository
{
    private readonly DtdDbContext _dbContext;

    public TemplateRepository(DtdDbContext dbContext) => _dbContext = dbContext;

    public async Task<Template?> GetByIdAsync(Guid templateId, CancellationToken cancellationToken = default) =>
        await _dbContext.Templates
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

    public async Task<Template?> GetByEmpresaYCodeAsync(
        string empresa, string code, CancellationToken cancellationToken = default) =>
        await _dbContext.Templates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Empresa == empresa && t.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Template>> ListarPorEmpresaAsync(
        string empresa, CancellationToken cancellationToken = default) =>
        await _dbContext.Templates.AsNoTracking()
            .Where(t => t.Empresa == empresa)
            .OrderBy(t => t.Code)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Template template, CancellationToken cancellationToken = default) =>
        await _dbContext.Templates.AddAsync(template, cancellationToken);

    /// <summary>La plantilla se carga con tracking en <see cref="GetByIdAsync"/>, por lo que los cambios
    /// se persisten con <c>SaveChanges</c>; este método queda como lugar canónico de la mutación.</summary>
    public Task UpdateAsync(Template template, CancellationToken cancellationToken = default) => Task.CompletedTask;
}