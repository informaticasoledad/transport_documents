using Dtd.Application.Almacenes.ListarConsigneesDefecto;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class ListarConsigneesDefectoQueryHandlerTests
{
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConsigneeRepository _consigneeRepository = Substitute.For<IConsigneeRepository>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    [Fact]
    public async Task Handle_devuelve_los_defaults_de_la_tupla()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Agencia 01"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(true);
        _consigneeRepository.ObtenerConsigneesDefectoAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(new List<Consignee>
            {
                Consignee.Crear("001", "CS01", "Dest 01", Canal.Create("email")!,
                    Movil.Create("600111222"), Email.Create("dest@x.com")),
                Consignee.Crear("001", "CS02", "Dest 02", Canal.Create("sms")!,
                    Movil.Create("600222333"), email: null)
            });
        var handler = new ListarConsigneesDefectoQueryHandler(_almacenRepository, _agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConsigneesDefectoQuery("001", "21", "AG01"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.Codigo == "CS01" && c.Channel == "email");
        result.Value.Should().Contain(c => c.Codigo == "CS02" && c.Channel == "sms");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_almacen_no_existe()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs((Almacen?)null);
        var handler = new ListarConsigneesDefectoQueryHandler(_almacenRepository, _agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConsigneesDefectoQuery("001", "99", "AG01"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.NoConfigurado");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_no_es_disponible()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG99", "Agencia 99"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(false);
        var handler = new ListarConsigneesDefectoQueryHandler(_almacenRepository, _agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConsigneesDefectoQuery("001", "21", "AG99"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.AgenciaNoDisponible");
        await _consigneeRepository.DidNotReceive().ObtenerConsigneesDefectoAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        var handler = new ListarConsigneesDefectoQueryHandler(_almacenRepository, _agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConsigneesDefectoQuery("001", "21", "AG01"), CancellationToken.None);

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
        _consigneeRepository.ObtenerConsigneesDefectoAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(Array.Empty<Consignee>());
        var handler = new ListarConsigneesDefectoQueryHandler(_almacenRepository, _agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(
            new ListarConsigneesDefectoQuery("001", "21", "AG01"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}