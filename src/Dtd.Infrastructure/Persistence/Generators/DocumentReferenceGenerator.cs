using System.Data;
using Dtd.Application.Documentos.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Persistence.Generators;

internal sealed class DocumentReferenceGenerator : IDocumentReferenceGenerator
{
    private readonly DtdDbContext _dbContext;

    public DocumentReferenceGenerator(DtdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(
        string empresa,
        string almacen,
        DateTime fecha,
        CancellationToken cancellationToken = default)
    {
        var numero = await GetNextSequenceValueAsync(cancellationToken);

        //return $"DDT/{empresa}/{almacen}/{fecha:yyyyMMdd}/{numero:000000}";
        return $"{empresa}/{almacen}/{fecha:yyyyMMdd}/{numero:000000}";
    }

    private async Task<long> GetNextSequenceValueAsync(
        CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();

        command.CommandText = "SELECT nextval('ddt_reference_seq')";

        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result is null || result == DBNull.Value)
        {
            throw new InvalidOperationException(
                "No se pudo obtener el siguiente valor de la secuencia 'ddt_reference_seq'.");
        }

        return Convert.ToInt64(result);
    }
}