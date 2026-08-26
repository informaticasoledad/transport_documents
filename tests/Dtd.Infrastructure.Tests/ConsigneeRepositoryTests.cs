using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos.ValueObjects;
using Dtd.Infrastructure.Persistence;
using Dtd.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Dtd.Infrastructure.Tests;

public class ConsigneeRepositoryTests
{
    private static DtdDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DtdDbContext>()
            .UseInMemoryDatabase($"dtd-consignees-{Guid.NewGuid()}")
            .Options;
        var ctx = new DtdDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private const string Empresa = "001";

    private static Consignee CrearConsignee(string codigo, bool activo = true, string channel = "sms")
    {
        var consignee = Consignee.Crear(
            Empresa, codigo, "Consignee " + codigo,
            Canal.Create(channel)!, Movil.Create("600000001"), email: null,
            taxId: "B87654321");
        if (!activo)
        {
            consignee.Desactivar();
        }
        return consignee;
    }

    private static ConsigneeAlmacen VincularAlmacen(Guid consigneeId, Guid almacenId) =>
        ConsigneeAlmacen.Crear(consigneeId, almacenId);

    private static ConsigneeAgencia VincularAgencia(Guid consigneeId, Guid agenciaId) =>
        ConsigneeAgencia.Crear(consigneeId, agenciaId);

    [Fact]
    public async Task GetByAlmacenYAgenciaEId_encuentra_si_vinculado_a_ambos_y_null_si_falta_alguno()
    {
        await using var ctx = CreateContext();
        var almacenId = Guid.NewGuid();
        var agenciaId = Guid.NewGuid();
        var consignee = CrearConsignee("C01");
        ctx.Consignees.Add(consignee);
        ctx.ConsigneeAlmacenes.Add(VincularAlmacen(consignee.Id, almacenId));
        ctx.ConsigneeAgencias.Add(VincularAgencia(consignee.Id, agenciaId));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var encontrado = await repo.GetByAlmacenYAgenciaEIdAsync(almacenId, agenciaId, consignee.Id, CancellationToken.None);
        var sinAlmacen = await repo.GetByAlmacenYAgenciaEIdAsync(Guid.NewGuid(), agenciaId, consignee.Id, CancellationToken.None);
        var sinAgencia = await repo.GetByAlmacenYAgenciaEIdAsync(almacenId, Guid.NewGuid(), consignee.Id, CancellationToken.None);
        var inexistente = await repo.GetByAlmacenYAgenciaEIdAsync(almacenId, agenciaId, Guid.NewGuid(), CancellationToken.None);

        encontrado.Should().NotBeNull();
        encontrado!.Codigo.Should().Be("C01");
        sinAlmacen.Should().BeNull("falta el vínculo al almacén");
        sinAgencia.Should().BeNull("falta el vínculo a la agencia");
        inexistente.Should().BeNull("no existe consignee con ese Id");
    }

    [Fact]
    public async Task GetByAlmacenYAgenciaEId_devuelve_el_consignee_aunque_este_inactivo()
    {
        await using var ctx = CreateContext();
        var almacenId = Guid.NewGuid();
        var agenciaId = Guid.NewGuid();
        var consignee = CrearConsignee("C01", activo: false);
        ctx.Consignees.Add(consignee);
        ctx.ConsigneeAlmacenes.Add(VincularAlmacen(consignee.Id, almacenId));
        ctx.ConsigneeAgencias.Add(VincularAgencia(consignee.Id, agenciaId));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var encontrado = await repo.GetByAlmacenYAgenciaEIdAsync(almacenId, agenciaId, consignee.Id, CancellationToken.None);
        encontrado.Should().NotBeNull("el handler distingue 404 vs Inactivo");
        encontrado!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ListarPorAlmacen_devuelve_solo_los_activos_vinculados()
    {
        await using var ctx = CreateContext();
        var almacenId = Guid.NewGuid();
        var otroAlmacenId = Guid.NewGuid();
        var c01 = CrearConsignee("C01");
        var c02 = CrearConsignee("C02");
        var c03Inactivo = CrearConsignee("C03", activo: false);
        var c04OtroAlmacen = CrearConsignee("C04"); // vinculado a otro almacén
        ctx.Consignees.AddRange(c01, c02, c03Inactivo, c04OtroAlmacen);
        ctx.ConsigneeAlmacenes.AddRange(
            VincularAlmacen(c01.Id, almacenId),
            VincularAlmacen(c02.Id, almacenId),
            VincularAlmacen(c03Inactivo.Id, almacenId),
            VincularAlmacen(c04OtroAlmacen.Id, otroAlmacenId));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var resultado = await repo.ListarPorAlmacenAsync(almacenId, CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(c => c.Activo);
        resultado.Should().Contain(c => c.Codigo == "C01");
        resultado.Should().Contain(c => c.Codigo == "C02");
        resultado.Should().NotContain(c => c.Codigo == "C04", "no está vinculado a este almacén");
    }

    [Fact]
    public async Task ListarPorAgencia_devuelve_solo_los_activos_vinculados()
    {
        await using var ctx = CreateContext();
        var agenciaId = Guid.NewGuid();
        var otraAgenciaId = Guid.NewGuid();
        var c01 = CrearConsignee("C01");
        var c02 = CrearConsignee("C02");
        var c03Inactivo = CrearConsignee("C03", activo: false);
        var c04OtraAgencia = CrearConsignee("C04"); // vinculado a otra agencia
        ctx.Consignees.AddRange(c01, c02, c03Inactivo, c04OtraAgencia);
        ctx.ConsigneeAgencias.AddRange(
            VincularAgencia(c01.Id, agenciaId),
            VincularAgencia(c02.Id, agenciaId),
            VincularAgencia(c03Inactivo.Id, agenciaId),
            VincularAgencia(c04OtraAgencia.Id, otraAgenciaId));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var resultado = await repo.ListarPorAgenciaAsync(agenciaId, CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(c => c.Activo);
        resultado.Should().Contain(c => c.Codigo == "C01");
        resultado.Should().Contain(c => c.Codigo == "C02");
        resultado.Should().NotContain(c => c.Codigo == "C04", "no está vinculado a esta agencia");
    }

    [Fact]
    public async Task ObtenerConsigneesDefecto_devuelve_los_defaults_activos_vinculados_a_ambos()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var c01 = CrearConsignee("C01");
        var c02 = CrearConsignee("C02");
        var c03Inactivo = CrearConsignee("C03", activo: false);
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.Consignees.AddRange(c01, c02, c03Inactivo);
        ctx.ConsigneeAlmacenes.AddRange(
            VincularAlmacen(c01.Id, almacen.Id),
            VincularAlmacen(c02.Id, almacen.Id),
            VincularAlmacen(c03Inactivo.Id, almacen.Id));
        ctx.ConsigneeAgencias.AddRange(
            VincularAgencia(c01.Id, agencia.Id),
            VincularAgencia(c02.Id, agencia.Id),
            VincularAgencia(c03Inactivo.Id, agencia.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agencia.Id));
        ctx.AlmacenAgenciaConsigneesDefecto.Add(AlmacenAgenciaConsigneeDefecto.Crear(almacen.Id, agencia.Id, c01.Id));
        ctx.AlmacenAgenciaConsigneesDefecto.Add(AlmacenAgenciaConsigneeDefecto.Crear(almacen.Id, agencia.Id, c02.Id));
        ctx.AlmacenAgenciaConsigneesDefecto.Add(AlmacenAgenciaConsigneeDefecto.Crear(almacen.Id, agencia.Id, c03Inactivo.Id));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var consignees = await repo.ObtenerConsigneesDefectoAsync("001", "21", "AG01", CancellationToken.None);

        consignees.Select(c => c.Codigo).Should().BeEquivalentTo(["C01", "C02"], "el default inactivo se omite");
        consignees.Should().OnlyContain(c => c.Activo);
    }

    [Fact]
    public async Task ObtenerConsigneesDefecto_excluye_un_default_cuyo_consignee_ya_no_esta_vinculado_a_ambos()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var c01 = CrearConsignee("C01");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.Consignees.Add(c01);
        // Default apunta a c01, vinculado al almacén pero NO a la agencia → se excluye (defense-in-depth).
        ctx.ConsigneeAlmacenes.Add(VincularAlmacen(c01.Id, almacen.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agencia.Id));
        ctx.AlmacenAgenciaConsigneesDefecto.Add(AlmacenAgenciaConsigneeDefecto.Crear(almacen.Id, agencia.Id, c01.Id));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var consignees = await repo.ObtenerConsigneesDefectoAsync("001", "21", "AG01", CancellationToken.None);

        consignees.Should().BeEmpty("el default apunta a un consignee no vinculado a la agencia");
    }

    [Fact]
    public async Task ObtenerConsigneesDefecto_devuelve_vacio_si_no_hay_defaults_o_no_existe_la_tupla()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, agencia.Id));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        (await repo.ObtenerConsigneesDefectoAsync("001", "21", "AG01", CancellationToken.None)).Should().BeEmpty("sin defaults configurados");
        (await repo.ObtenerConsigneesDefectoAsync("001", "99", "AG01", CancellationToken.None)).Should().BeEmpty("almacén inexistente");
        (await repo.ObtenerConsigneesDefectoAsync("002", "21", "AG01", CancellationToken.None)).Should().BeEmpty("empresa distinta");
    }

    [Fact]
    public async Task ObtenerConsigneesDefecto_especifico_de_la_tupla_almacen_agencia()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var ag01 = Agencia.Crear("001", "AG01", "Pepe");
        var ag02 = Agencia.Crear("001", "AG02", "Express");
        var c01 = CrearConsignee("C01");
        var c02 = CrearConsignee("C02");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.AddRange(ag01, ag02);
        ctx.Consignees.AddRange(c01, c02);
        ctx.ConsigneeAlmacenes.AddRange(
            VincularAlmacen(c01.Id, almacen.Id),
            VincularAlmacen(c02.Id, almacen.Id));
        ctx.ConsigneeAgencias.AddRange(
            VincularAgencia(c01.Id, ag01.Id),
            VincularAgencia(c02.Id, ag02.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, ag01.Id));
        ctx.AlmacenAgencias.Add(AlmacenAgencia.Crear(almacen.Id, ag02.Id));
        ctx.AlmacenAgenciaConsigneesDefecto.Add(AlmacenAgenciaConsigneeDefecto.Crear(almacen.Id, ag01.Id, c01.Id));
        ctx.AlmacenAgenciaConsigneesDefecto.Add(AlmacenAgenciaConsigneeDefecto.Crear(almacen.Id, ag02.Id, c02.Id));
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        var defaultsAg01 = await repo.ObtenerConsigneesDefectoAsync("001", "21", "AG01", CancellationToken.None);
        defaultsAg01.Select(c => c.Codigo).Should().BeEquivalentTo(["C01"]);

        var defaultsAg02 = await repo.ObtenerConsigneesDefectoAsync("001", "21", "AG02", CancellationToken.None);
        defaultsAg02.Select(c => c.Codigo).Should().BeEquivalentTo(["C02"]);
    }

    [Fact]
    public async Task AddAsync_persiste_el_consignee_y_sus_vinculos_con_almacenes_y_agencias()
    {
        await using var ctx = CreateContext();
        var alm1 = Guid.NewGuid();
        var alm2 = Guid.NewGuid();
        var ag1 = Guid.NewGuid();
        var ag2 = Guid.NewGuid();
        var consignee = CrearConsignee("C01");
        var repo = new ConsigneeRepository(ctx);

        await repo.AddAsync(consignee, [alm1, alm2, alm1], [ag1, ag2, ag1], CancellationToken.None);
        await ctx.SaveChangesAsync();

        (await ctx.Consignees.FindAsync(consignee.Id)).Should().NotBeNull();
        var vinculosAlmacen = await ctx.ConsigneeAlmacenes.Where(v => v.ConsigneeId == consignee.Id).ToListAsync();
        vinculosAlmacen.Should().HaveCount(2, "los almacenIds se deduplican");
        vinculosAlmacen.Select(v => v.AlmacenId).Should().BeEquivalentTo([alm1, alm2]);
        var vinculosAgencia = await ctx.ConsigneeAgencias.Where(v => v.ConsigneeId == consignee.Id).ToListAsync();
        vinculosAgencia.Should().HaveCount(2, "los agenciaIds se deduplican");
        vinculosAgencia.Select(v => v.AgenciaId).Should().BeEquivalentTo([ag1, ag2]);
    }

    [Fact]
    public async Task ActualizarAsync_reemplaza_los_vinculos()
    {
        await using var ctx = CreateContext();
        var alm1 = Guid.NewGuid();
        var alm2 = Guid.NewGuid();
        var ag1 = Guid.NewGuid();
        var ag2 = Guid.NewGuid();
        var consignee = CrearConsignee("CS01");
        var repo = new ConsigneeRepository(ctx);

        await repo.AddAsync(consignee, [alm1], [ag1], CancellationToken.None);
        await ctx.SaveChangesAsync();

        // ActualizarAsync usa consignee.Id para query los vínculos; cualquier instancia con el Id correcto vale.
        var tracked = ctx.Consignees.First(c => c.Codigo == "CS01");
        await repo.ActualizarAsync(tracked, [alm2], [ag2], CancellationToken.None);
        await ctx.SaveChangesAsync();

        var vinculosAlmacen = await ctx.ConsigneeAlmacenes.Where(v => v.ConsigneeId == consignee.Id).ToListAsync();
        vinculosAlmacen.Should().HaveCount(1);
        vinculosAlmacen[0].AlmacenId.Should().Be(alm2);
        var vinculosAgencia = await ctx.ConsigneeAgencias.Where(v => v.ConsigneeId == consignee.Id).ToListAsync();
        vinculosAgencia.Should().HaveCount(1);
        vinculosAgencia[0].AgenciaId.Should().Be(ag2);
    }

    [Fact]
    public async Task SetDefectosAsync_reemplaza_los_defaults()
    {
        await using var ctx = CreateContext();
        var almacen = Almacen.Crear("001", "21", "GETAFE");
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        var c01 = CrearConsignee("C01");
        var c02 = CrearConsignee("C02");
        ctx.Almacenes.Add(almacen);
        ctx.Agencias.Add(agencia);
        ctx.Consignees.AddRange(c01, c02);
        await ctx.SaveChangesAsync();

        var repo = new ConsigneeRepository(ctx);

        await repo.SetDefectosAsync("001", "21", "AG01", [c01.Id, c02.Id], CancellationToken.None);
        await ctx.SaveChangesAsync();
        (await ctx.AlmacenAgenciaConsigneesDefecto
            .Where(d => d.AlmacenId == almacen.Id && d.AgenciaId == agencia.Id)
            .ToListAsync())
            .Should().HaveCount(2);

        await repo.SetDefectosAsync("001", "21", "AG01", [c01.Id], CancellationToken.None);
        await ctx.SaveChangesAsync();
        (await ctx.AlmacenAgenciaConsigneesDefecto
            .Where(d => d.AlmacenId == almacen.Id && d.AgenciaId == agencia.Id)
            .ToListAsync())
            .Should().HaveCount(1, "c02 ya no es default");

        await repo.SetDefectosAsync("001", "21", "AG01", Array.Empty<Guid>(), CancellationToken.None);
        await ctx.SaveChangesAsync();
        (await ctx.AlmacenAgenciaConsigneesDefecto
            .Where(d => d.AlmacenId == almacen.Id && d.AgenciaId == agencia.Id)
            .ToListAsync())
            .Should().BeEmpty("lista vacía limpia los defaults");
    }
}
