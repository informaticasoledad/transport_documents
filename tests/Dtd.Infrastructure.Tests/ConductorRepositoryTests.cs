using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Infrastructure.Persistence;
using Dtd.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Tests;

public class ConductorRepositoryTests
{
    private static DtdDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DtdDbContext>()
            .UseInMemoryDatabase($"dtd-conductores-{Guid.NewGuid()}")
            .Options;
        var ctx = new DtdDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private const string Empresa = "001";

    private static Conductor CrearConductor(string codigo, bool activo = true, string channel = "sms")
    {
        var conductor = Conductor.Crear(
            Empresa, codigo, "Conductor " + codigo,
            Canal.Create(channel)!, Movil.Create("600000001"), email: null,
            taxId: "12345678Z", licensePlate: "1234ABC");
        if (!activo)
        {
            conductor.Desactivar();
        }
        return conductor;
    }

    private static ConductorAgencia Vincular(Guid conductorId, Guid agenciaId) =>
        new() { ConductorId = conductorId, AgenciaId = agenciaId };

    [Fact]
    public async Task GetByAgenciaYId_encuentra_si_esta_vinculado_y_devuelve_null_si_no()
    {
        await using var ctx = CreateContext();
        var agenciaId = Guid.NewGuid();
        var conductor = CrearConductor("C01");
        ctx.Conductores.Add(conductor);
        ctx.ConductorAgencias.Add(Vincular(conductor.Id, agenciaId));
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        var encontrado = await repo.GetByAgenciaYIdAsync(agenciaId, conductor.Id, CancellationToken.None);
        var ausente = await repo.GetByAgenciaYIdAsync(agenciaId, Guid.NewGuid(), CancellationToken.None);
        var otraAgencia = await repo.GetByAgenciaYIdAsync(Guid.NewGuid(), conductor.Id, CancellationToken.None);

        encontrado.Should().NotBeNull();
        encontrado!.Codigo.Should().Be("C01");
        ausente.Should().BeNull("no existe conductor con ese Id");
        otraAgencia.Should().BeNull("el conductor no está vinculado a esa agencia");
    }

    [Fact]
    public async Task GetByAgenciaYId_devuelve_el_conductor_aunque_este_inactivo()
    {
        await using var ctx = CreateContext();
        var agenciaId = Guid.NewGuid();
        var conductor = CrearConductor("C01", activo: false);
        ctx.Conductores.Add(conductor);
        ctx.ConductorAgencias.Add(Vincular(conductor.Id, agenciaId));
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        var encontrado = await repo.GetByAgenciaYIdAsync(agenciaId, conductor.Id, CancellationToken.None);
        encontrado.Should().NotBeNull("el handler distingue 404 vs Inactivo");
        encontrado!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ListarPorAgencia_devuelve_solo_los_activos_vinculados_a_la_agencia()
    {
        await using var ctx = CreateContext();
        var agenciaId = Guid.NewGuid();
        var otraAgenciaId = Guid.NewGuid();
        var c01 = CrearConductor("C01");
        var c02 = CrearConductor("C02");
        var c03Inactivo = CrearConductor("C03", activo: false);
        var c04OtraAgencia = CrearConductor("C04"); // vinculado a otra agencia
        ctx.Conductores.AddRange(c01, c02, c03Inactivo, c04OtraAgencia);
        ctx.ConductorAgencias.AddRange(
            Vincular(c01.Id, agenciaId),
            Vincular(c02.Id, agenciaId),
            Vincular(c03Inactivo.Id, agenciaId),
            Vincular(c04OtraAgencia.Id, otraAgenciaId));
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        var resultado = await repo.ListarPorAgenciaAsync(agenciaId, CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(c => c.Activo);
        resultado.Should().Contain(c => c.Codigo == "C01");
        resultado.Should().Contain(c => c.Codigo == "C02");
        resultado.Should().NotContain(c => c.Codigo == "C04", "no está vinculado a esta agencia");
    }

    [Fact]
    public async Task ObtenerConductoresDefecto_devuelve_los_defaults_activos_vinculados_de_la_tupla()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var c01 = CrearConductor("C01");
        var c02 = CrearConductor("C02");
        var c03Inactivo = CrearConductor("C03", activo: false);
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.Conductores.AddRange(c01, c02, c03Inactivo);
        ctx.ConductorAgencias.AddRange(Vincular(c01.Id, agencia.Id), Vincular(c02.Id, agencia.Id), Vincular(c03Inactivo.Id, agencia.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agencia.Id));
        ctx.AlmacenAgenciaConductoresDefecto.Add(new AlmacenAgenciaConductorDefecto { AlmacenId = almacen.Id, AgenciaId = agencia.Id, ConductorId = c01.Id });
        ctx.AlmacenAgenciaConductoresDefecto.Add(new AlmacenAgenciaConductorDefecto { AlmacenId = almacen.Id, AgenciaId = agencia.Id, ConductorId = c02.Id });
        ctx.AlmacenAgenciaConductoresDefecto.Add(new AlmacenAgenciaConductorDefecto { AlmacenId = almacen.Id, AgenciaId = agencia.Id, ConductorId = c03Inactivo.Id });
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        var conductores = await repo.ObtenerConductoresDefectoAsync("001", "21", "AG01", CancellationToken.None);

        conductores.Select(c => c.Codigo).Should().BeEquivalentTo(["C01", "C02"], "el default inactivo se omite");
        conductores.Should().OnlyContain(c => c.Activo);
    }

    [Fact]
    public async Task ObtenerConductoresDefecto_excluye_un_default_cuyo_conductor_ya_no_esta_vinculado_a_la_agencia()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var c01 = CrearConductor("C01");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.Conductores.Add(c01);
        // Default apunta a c01, pero c01 NO está vinculado a la agencia → se excluye (defense-in-depth).
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agencia.Id));
        ctx.AlmacenAgenciaConductoresDefecto.Add(new AlmacenAgenciaConductorDefecto { AlmacenId = almacen.Id, AgenciaId = agencia.Id, ConductorId = c01.Id });
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        var conductores = await repo.ObtenerConductoresDefectoAsync("001", "21", "AG01", CancellationToken.None);

        conductores.Should().BeEmpty("el default apunta a un conductor no vinculado a la agencia");
    }

    [Fact]
    public async Task ObtenerConductoresDefecto_devuelve_vacio_si_no_hay_defaults_o_no_existe_la_tupla()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agencia.Id));
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        (await repo.ObtenerConductoresDefectoAsync("001", "21", "AG01", CancellationToken.None)).Should().BeEmpty("sin defaults configurados");
        (await repo.ObtenerConductoresDefectoAsync("001", "99", "AG01", CancellationToken.None)).Should().BeEmpty("almacén inexistente");
        (await repo.ObtenerConductoresDefectoAsync("002", "21", "AG01", CancellationToken.None)).Should().BeEmpty("empresa distinta");
    }

    [Fact]
    public async Task ObtenerConductoresDefecto_especifico_de_la_tupla_almacen_agencia()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var ag01 = Agencia.Crear("001", "AG01", "Pepe");
        var ag02 = Agencia.Crear("001", "AG02", "Express");
        var c01 = CrearConductor("C01");
        var c02 = CrearConductor("C02");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.AddRange(ag01, ag02);
        ctx.Conductores.AddRange(c01, c02);
        ctx.ConductorAgencias.AddRange(Vincular(c01.Id, ag01.Id), Vincular(c02.Id, ag02.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, ag01.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, ag02.Id));
        ctx.AlmacenAgenciaConductoresDefecto.Add(new AlmacenAgenciaConductorDefecto { AlmacenId = almacen.Id, AgenciaId = ag01.Id, ConductorId = c01.Id });
        ctx.AlmacenAgenciaConductoresDefecto.Add(new AlmacenAgenciaConductorDefecto { AlmacenId = almacen.Id, AgenciaId = ag02.Id, ConductorId = c02.Id });
        await ctx.SaveChangesAsync();

        var repo = new ConductorRepository(ctx);

        var defaultsAg01 = await repo.ObtenerConductoresDefectoAsync("001", "21", "AG01", CancellationToken.None);
        defaultsAg01.Select(c => c.Codigo).Should().BeEquivalentTo(["C01"]);

        var defaultsAg02 = await repo.ObtenerConductoresDefectoAsync("001", "21", "AG02", CancellationToken.None);
        defaultsAg02.Select(c => c.Codigo).Should().BeEquivalentTo(["C02"]);
    }

    [Fact]
    public async Task AddAsync_persiste_el_conductor_y_sus_vinculos_con_agencias()
    {
        await using var ctx = CreateContext();
        var ag01 = Guid.NewGuid();
        var ag02 = Guid.NewGuid();
        var conductor = CrearConductor("C01");
        var repo = new ConductorRepository(ctx);

        await repo.AddAsync(conductor, [ag01, ag02, ag01], CancellationToken.None);
        await ctx.SaveChangesAsync();

        (await ctx.Conductores.FindAsync(conductor.Id)).Should().NotBeNull();
        var vinculos = await ctx.ConductorAgencias.Where(v => v.ConductorId == conductor.Id).ToListAsync();
        vinculos.Should().HaveCount(2, "los agenciaIds se deduplican");
        vinculos.Select(v => v.AgenciaId).Should().BeEquivalentTo([ag01, ag02]);
    }
}
