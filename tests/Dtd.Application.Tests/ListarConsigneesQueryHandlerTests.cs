using Dtd.Application.Consignees.ListarConsignees;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class ListarConsigneesQueryHandlerTests
{
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConsigneeRepository _consigneeRepository = Substitute.For<IConsigneeRepository>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    [Fact]
    public async Task Handle_devuelve_los_consignees_activos_de_la_agencia()
    {
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(agencia);
        _consigneeRepository.ListarPorAgenciaAsync(default, default)
            .ReturnsForAnyArgs(new List<Consignee>
            {
                Consignee.Crear("001", "CS01", "Dest 01", Canal.Create("email")!,
                    Movil.Create("600111222"), Email.Create("dest@x.com")),
                Consignee.Crear("001", "CS02", "Dest 02", Canal.Create("sms")!,
                    Movil.Create("600222333"), email: null)
            });
        var handler = new ListarConsigneesQueryHandler(_agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarConsigneesQuery("001", "AG01"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.Codigo == "CS01" && c.Channel == "email" && c.Email == "dest@x.com");
        result.Value.Should().Contain(c => c.Codigo == "CS02" && c.Channel == "sms");
        result.Value.Should().OnlyContain(c => c.Activo);
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_no_existe()
    {
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs((Agencia?)null);
        var handler = new ListarConsigneesQueryHandler(_agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarConsigneesQuery("001", "AG99"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Agencia.NoEncontrada");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        var handler = new ListarConsigneesQueryHandler(_agenciaRepository, _consigneeRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarConsigneesQuery("001", "AG01"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }
}