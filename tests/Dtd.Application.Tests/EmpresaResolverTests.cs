using Dtd.Application.GatewayContracts;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dtd.Application.Tests;

public class EmpresaResolverTests
{
    // Empresa 001 con configuración completa. El resolver no valida el formato de empresa (es solo un
    // lookup por clave natural); la normalización vive en el límite del API. EmpresaConfig solo
    // lleva base_address: el resto del cliente OAuth2 (token_endpoint, client_id, scope, client_secret)
    // es común a todas las empresas y va en appsettings (ErpOptions), no en la tabla.
    private static EmpresaConfig Config(string empresa) =>
        new(empresa, "https://erp.example.local/");

    private static EmpresaResolver CreateResolver(IEmpresaRepository repo, TimeSpan ttl) =>
        new(repo, new MemoryCache(Options.Create(new MemoryCacheOptions())), ttl);

    [Fact]
    public async Task ResolveAsync_recupera_del_repo_y_lo_cachea()
    {
        var repo = Substitute.For<IEmpresaRepository>();
        repo.GetByEmpresaAsync("001", Arg.Any<CancellationToken>())
            .Returns(Config("001"));
        var resolver = CreateResolver(repo, TimeSpan.FromMinutes(2));

        var first = await resolver.ResolveAsync("001");
        var second = await resolver.ResolveAsync("001");

        first.Should().NotBeNull();
        first!.BaseAddress.Should().Be("https://erp.example.local/");
        // El segundo resolve no vuelve a tocar el repositorio (caché).
        await repo.Received(1).GetByEmpresaAsync("001", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_cashea_el_no_configurado_para_no_machacar_la_bbdd()
    {
        var repo = Substitute.For<IEmpresaRepository>();
        repo.GetByEmpresaAsync("998", Arg.Any<CancellationToken>())
            .Returns((EmpresaConfig?)null);
        var resolver = CreateResolver(repo, TimeSpan.FromMinutes(2));

        var first = await resolver.ResolveAsync("998");
        var second = await resolver.ResolveAsync("998");

        first.Should().BeNull();
        second.Should().BeNull();
        // Ambas llamadas devuelven null pero el repo sólo se consulta una vez.
        await repo.Received(1).GetByEmpresaAsync("998", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_reconsulta_el_repo_tras_expirar_el_ttl()
    {
        var repo = Substitute.For<IEmpresaRepository>();
        repo.GetByEmpresaAsync("001", Arg.Any<CancellationToken>())
            .Returns(Config("001"));
        var resolver = CreateResolver(repo, TimeSpan.FromMilliseconds(150));

        await resolver.ResolveAsync("001");
        await Task.Delay(350);
        await resolver.ResolveAsync("001");

        // Tras expirar el TTL, el repo se vuelve a consultar.
        await repo.Received(2).GetByEmpresaAsync("001", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_no_mezcla_empresas()
    {
        var repo = Substitute.For<IEmpresaRepository>();
        repo.GetByEmpresaAsync("001", Arg.Any<CancellationToken>()).Returns(Config("001"));
        repo.GetByEmpresaAsync("002", Arg.Any<CancellationToken>()).Returns((EmpresaConfig?)null);
        var resolver = CreateResolver(repo, TimeSpan.FromMinutes(2));

        var a = await resolver.ResolveAsync("001");
        var b = await resolver.ResolveAsync("002");

        a.Should().NotBeNull();
        b.Should().BeNull();
        await repo.Received(1).GetByEmpresaAsync("001", Arg.Any<CancellationToken>());
        await repo.Received(1).GetByEmpresaAsync("002", Arg.Any<CancellationToken>());
    }
}