using Dtd.Application.Almacenes.ListarConductoresDefecto;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class ListarConductoresDefectoQueryHandlerTests
{
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConductorRepository _conductorRepository = Substitute.For<IConductorRepository>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    [Fact]
    public async Task Handle_devuelve_los_defaults_de_la_tupla_almacen_agencia()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Agencia 01"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(true);
        _conductorRepository.ObtenerConductoresDefectoAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(new List<Conductor>
            {
                Conductor.Crear("001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null),
                Conductor.Crear("001", "C02", "Ana", Canal.Create("email")!, movil: null, email: Email.Create("ana@x.com"))
            });
        var handler = new ListarConductoresDefectoQueryHandler(_almacenRepository, _agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConductoresDefectoQuery("001", "21", "AG01"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.Codigo == "C01" && c.Channel == "sms");
        result.Value.Should().Contain(c => c.Codigo == "C02" && c.Channel == "email" && c.Email == "ana@x.com");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_almacen_no_existe()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs((Almacen?)null);
        var handler = new ListarConductoresDefectoQueryHandler(_almacenRepository, _agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConductoresDefectoQuery("001", "99", "AG01"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.NoConfigurado");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_no_es_disponible_para_el_almacen()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG99", "Agencia 99"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(false);
        var handler = new ListarConductoresDefectoQueryHandler(_almacenRepository, _agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConductoresDefectoQuery("001", "21", "AG99"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.AgenciaNoDisponible");
        await _conductorRepository.DidNotReceive().ObtenerConductoresDefectoAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        var handler = new ListarConductoresDefectoQueryHandler(_almacenRepository, _agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConductoresDefectoQuery("001", "21", "AG01"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_devuelve_lista_vacia_si_no_hay_defaults()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Agencia 01"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(true);
        _conductorRepository.ObtenerConductoresDefectoAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(Array.Empty<Conductor>());
        var handler = new ListarConductoresDefectoQueryHandler(_almacenRepository, _agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConductoresDefectoQuery("001", "21", "AG01"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}