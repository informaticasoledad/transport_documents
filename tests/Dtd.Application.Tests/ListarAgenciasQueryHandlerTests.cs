using Dtd.Application.Agencias.ListarAgencias;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class ListarAgenciasQueryHandlerTests
{
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    [Fact]
    public async Task Handle_devuelve_las_agencias_activas_de_la_empresa()
    {
        _agenciaRepository.ListarPorEmpresaAsync(default!, default)
            .ReturnsForAnyArgs(new List<Agencia>
            {
                Agencia.Crear("001", "AG01", "Transportes Pepe"),
                Agencia.Crear("001", "AG02", "Express")
            });
        var handler = new ListarAgenciasQueryHandler(_agenciaRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarAgenciasQuery("1"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(a => a.Codigo == "AG01" && a.Nombre == "Transportes Pepe");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        var handler = new ListarAgenciasQueryHandler(_agenciaRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarAgenciasQuery("001"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
        result.Errors[0].Code.Should().Be("Empresa.NoAutorizada");
        await _agenciaRepository.DidNotReceive().ListarPorEmpresaAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}