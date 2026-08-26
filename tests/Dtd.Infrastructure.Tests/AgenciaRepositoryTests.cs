using Dtd.Domain.Agencias;
using Dtd.Infrastructure.Persistence;
using Dtd.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Tests;

public class AgenciaRepositoryTests
{
    private static DtdDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DtdDbContext>()
            .UseInMemoryDatabase($"dtd-agencias-{Guid.NewGuid()}")
            .Options;
        var ctx = new DtdDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task GetByEmpresaYCodigo_encuentra_por_empresa_y_codigo_y_devuelve_null_si_no_existe()
    {
        await using var ctx = CreateContext();
        ctx.Agencias.Add(Agencia.Crear("001", "AG01", "Transportes Pepe"));
        await ctx.SaveChangesAsync();

        var repo = new AgenciaRepository(ctx);

        var encontrado = await repo.GetByEmpresaYCodigoAsync("001", "AG01", CancellationToken.None);
        var otraEmpresa = await repo.GetByEmpresaYCodigoAsync("002", "AG01", CancellationToken.None);
        var ausente = await repo.GetByEmpresaYCodigoAsync("001", "AG99", CancellationToken.None);

        encontrado.Should().NotBeNull();
        encontrado!.Codigo.Should().Be("AG01");
        otraEmpresa.Should().BeNull("el código AG01 existe para 001, no para 002");
        ausente.Should().BeNull();
    }

    [Fact]
    public async Task ListarPorEmpresa_devuelve_solo_las_activas_de_la_empresa()
    {
        await using var ctx = CreateContext();
        ctx.Agencias.Add(Agencia.Crear("001", "AG01", "Pepe"));
        var inactiva = Agencia.Crear("001", "AG02", "Express");
        inactiva.Desactivar();
        ctx.Agencias.Add(inactiva);
        ctx.Agencias.Add(Agencia.Crear("002", "AG01", "Otra empresa"));
        await ctx.SaveChangesAsync();

        var repo = new AgenciaRepository(ctx);

        var resultado = await repo.ListarPorEmpresaAsync("001", CancellationToken.None);

        resultado.Should().ContainSingle().Which.Codigo.Should().Be("AG01");
    }
}