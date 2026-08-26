using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Infrastructure.Persistence;
using Dtd.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Tests;

public class AlmacenRepositoryTests
{
    private static DtdDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DtdDbContext>()
            .UseInMemoryDatabase($"dtd-almacen-{Guid.NewGuid()}")
            .Options;
        var ctx = new DtdDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task GetByEmpresaYCodigo_encuentra_por_clave_natural_y_devuelve_null_si_no_existe()
    {
        await using var ctx = CreateContext();
        ctx.Almacenes.Add(Almacen.Crear("001", "21", "GETAFE"));
        await ctx.SaveChangesAsync();

        var repo = new AlmacenRepository(ctx);

        var encontrado = await repo.GetByEmpresaYCodigoAsync("001", "21", CancellationToken.None);
        var ausente = await repo.GetByEmpresaYCodigoAsync("002", "21", CancellationToken.None);

        encontrado.Should().NotBeNull();
        encontrado!.Codigo.Should().Be("21");
        ausente.Should().BeNull();
    }

    [Fact]
    public async Task ListarPorEmpresa_devuelve_solo_los_activos_de_la_empresa()
    {
        await using var ctx = CreateContext();
        ctx.Almacenes.Add(Almacen.Crear("001", "21", "GETAFE"));
        var inactivo = Almacen.Crear("001", "22", "CERRADO");
        inactivo.Desactivar();
        ctx.Almacenes.Add(inactivo);
        ctx.Almacenes.Add(Almacen.Crear("002", "30", "OTRA EMPRESA"));
        await ctx.SaveChangesAsync();

        var repo = new AlmacenRepository(ctx);

        var resultado = await repo.ListarPorEmpresaAsync("001", CancellationToken.None);

        resultado.Should().ContainSingle().Which.Codigo.Should().Be("21");
    }

    [Fact]
    public async Task ListarAgenciasDisponibles_devuelve_solo_las_vinculadas_y_activas()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agVinculadaActiva = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var agVinculadaInactiva = Agencia.Crear("001", "AG02", "Express");
        agVinculadaInactiva.Desactivar();
        var agNoVinculada = Agencia.Crear("001", "AG03", "Sin enlace");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agVinculadaActiva);
        ctx.Agencias.Add(agVinculadaInactiva);
        ctx.Agencias.Add(agNoVinculada);
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agVinculadaActiva.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agVinculadaInactiva.Id));
        await ctx.SaveChangesAsync();

        var repo = new AlmacenRepository(ctx);

        var disponibles = await repo.ListarAgenciasDisponiblesAsync("001", "21", CancellationToken.None);

        disponibles.Should().ContainSingle().Which.Codigo.Should().Be("AG01");
    }

    [Fact]
    public async Task EsAgenciaDisponible_refleja_vinculo_y_estado_de_la_agencia()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agActiva = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var agInactiva = Agencia.Crear("001", "AG02", "Express");
        agInactiva.Desactivar();
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agActiva);
        ctx.Agencias.Add(agInactiva);
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agActiva.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agInactiva.Id));
        await ctx.SaveChangesAsync();

        var repo = new AlmacenRepository(ctx);

        (await repo.EsAgenciaDisponibleAsync(almacen.Id, agActiva.Id, CancellationToken.None)).Should().BeTrue();
        (await repo.EsAgenciaDisponibleAsync(almacen.Id, agInactiva.Id, CancellationToken.None)).Should().BeFalse("la agencia está inactiva");
        (await repo.EsAgenciaDisponibleAsync(almacen.Id, Guid.NewGuid(), CancellationToken.None)).Should().BeFalse("no está vinculada al almacén");
        (await repo.EsAgenciaDisponibleAsync(Guid.NewGuid(), agActiva.Id, CancellationToken.None)).Should().BeFalse("el almacén no existe");
    }
}
