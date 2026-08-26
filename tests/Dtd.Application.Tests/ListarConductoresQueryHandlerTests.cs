using Dtd.Application.Conductores.ListarConductores;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Dtd.Application.Tests;

public class ListarConductoresQueryHandlerTests
{
    private readonly IAgenciaRepository _agenciaRepository = Substitute.For<IAgenciaRepository>();
    private readonly IConductorRepository _conductorRepository = Substitute.For<IConductorRepository>();
    private readonly IUsuarioContexto _usuarioContexto = Substitute.For<IUsuarioContexto>();

    [Fact]
    public async Task Handle_devuelve_los_conductores_activos_de_la_agencia()
    {
        var agencia = Agencia.Crear("001", "AG01", "Transportes Pepe");
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs(agencia);
        // ReturnsForAnyArgs: el Id concreto de la agencia devuelta por el stub superior es irrelevante.
        _conductorRepository.ListarPorAgenciaAsync(default, default)
            .ReturnsForAnyArgs(new List<Conductor>
            {
                Conductor.Crear("001", "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null,
                    taxId: "12345678Z", licensePlate: "1234ABC"),
                Conductor.Crear("001", "C02", "Ana", Canal.Create("email")!, movil: null, email: Email.Create("ana@x.com"))
            });
        var handler = new ListarConductoresQueryHandler(_agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarConductoresQuery("1", "AG01"), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(c => c.Codigo == "C01" && c.TaxId == "12345678Z" && c.LicensePlate == "1234ABC" && c.Channel == "sms");
        result.Value.Should().Contain(c => c.Codigo == "C02" && c.Channel == "email" && c.Email == "ana@x.com");
    }

    [Fact]
    public async Task Handle_devuelve_not_found_si_la_agencia_no_existe()
    {
        _agenciaRepository.GetByEmpresaYCodigoAsync(default!, default!, default)
            .ReturnsForAnyArgs((Agencia?)null);
        var handler = new ListarConductoresQueryHandler(_agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarConductoresQuery("001", "AG99"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Code.Should().Be("Agencia.NoEncontrada");
    }

    [Fact]
    public async Task Handle_devuelve_forbidden_si_el_usuario_no_tiene_acceso()
    {
        _usuarioContexto.Current.Returns(new UsuarioInfo("u", "Fran", new HashSet<string> { "002" }));
        var handler = new ListarConductoresQueryHandler(_agenciaRepository, _conductorRepository, _usuarioContexto);

        var result = await handler.Handle(new ListarConductoresQuery("001", "AG01"), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Forbidden);
    }
}