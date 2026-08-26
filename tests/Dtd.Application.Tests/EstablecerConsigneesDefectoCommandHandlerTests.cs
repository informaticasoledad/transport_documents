using Dtd.Application.Consignees;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class EstablecerConsigneesDefectoCommandHandlerTests
{
    private readonly IAlmacenRepository _almacenRepository = Substitute.For<IAlmacenRepository>();
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConsigneeRepository _consigneeRepository = Substitute.For<IConsigneeRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    private readonly EstablecerConsigneesDefectoCommandHandler _handler;

    public EstablecerConsigneesDefectoCommandHandlerTests()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Almacen.Crear("001", "21", "GETAFE"));
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(Agencia.Crear("001", "AG01", "Pepe"));
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(true);

        _handler = new EstablecerConsigneesDefectoCommandHandler(
            _almacenRepository, _agenciaRepository, _consigneeRepository, _unitOfWork, _usuarioContexto);
    }

    private static Consignee CrearConsignee(string codigo) =>
        Consignee.Crear("001", codigo, "Consignee " + codigo, Canal.Create("email")!,
            Movil.Create("600111222"), Email.Create("dest@x.com"));

    private static EstablecerConsigneesDefectoCommand Command(IReadOnlyList<Guid> consigneeIds) =>
        new("001", "21", "AG01", consigneeIds);

    [Fact]
    public async Task Handle_establece_los_defaults_y_devuelve_los_activos_vinculados()
    {
        var cs01 = CrearConsignee("CS01");
        var cs02 = CrearConsignee("CS02");
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs(callInfo =>
            {
                var id = callInfo.ArgAt<Guid>(2);
                if (id == cs01.Id) return cs01;
                if (id == cs02.Id) return cs02;
                return (Consignee?)null;
            });
        _consigneeRepository.ObtenerConsigneesDefectoAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(new List<Consignee> { cs01, cs02 });

        var result = await _handler.Handle(Command([cs01.Id, cs02.Id]), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        await _consigneeRepository.Received(1).SetDefectosAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is<IReadOnlyCollection<Guid>>(c => c.Count == 2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_con_lista_vacia_limpia_los_defaults()
    {
        var result = await _handler.Handle(Command(Array.Empty<Guid>()), CancellationToken.None);

        result.IsError.Should().BeFalse();
        await _consigneeRepository.Received(1).SetDefectosAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is<IReadOnlyCollection<Guid>>(c => c.Count == 0),
            Arg.Any<CancellationToken>());
        await _consigneeRepository.DidNotReceive().GetByAlmacenYAgenciaEIdAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_un_consignee_no_esta_vinculado_a_ambos()
    {
        var cs01 = CrearConsignee("CS01");
        var badId = Guid.NewGuid();
        _consigneeRepository.GetByAlmacenYAgenciaEIdAsync(default, default, default, default)
            .ReturnsForAnyArgs(callInfo =>
            {
                var id = callInfo.ArgAt<Guid>(2);
                if (id == cs01.Id) return cs01;
                return (Consignee?)null;
            });

        var result = await _handler.Handle(Command([cs01.Id, badId]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Consignee.NoVinculado");
        await _consigneeRepository.DidNotReceive().SetDefectosAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_el_almacen_no_existe()
    {
        _almacenRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs((Almacen?)null);

        var result = await _handler.Handle(Command([Guid.NewGuid()]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.NoConfigurado");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_no_es_disponible()
    {
        _almacenRepository.EsAgenciaDisponibleAsync(default, default, default)
            .ReturnsForAnyArgs(false);

        var result = await _handler.Handle(Command([Guid.NewGuid()]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Almacen.AgenciaNoDisponible");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));

        var result = await _handler.Handle(Command([Guid.NewGuid()]), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }
}