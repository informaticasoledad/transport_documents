using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using FluentAssertions;

namespace Dtd.Domain.Tests;

public class ConductorTests
{
    private const string Empresa = "001";

    [Fact]
    public void Crear_con_canal_sms_y_movil_se_crea_activo()
    {
        var conductor = Conductor.Crear(
            Empresa, "C01", "Pepe",
            Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            taxId: "12345678Z", licensePlate: "1234ABC");

        conductor.Id.Should().NotBeEmpty();
        conductor.Empresa.Should().Be("001");
        conductor.Codigo.Should().Be("C01");
        conductor.Nombre.Should().Be("Pepe");
        conductor.TaxId.Should().Be("12345678Z");
        conductor.LicensePlate.Should().Be("1234ABC");
        conductor.Canal.Valor.Should().Be("sms");
        conductor.Movil!.Valor.Should().Be("600000001");
        conductor.Email.Should().BeNull();
        conductor.Language.Should().Be("es");
        conductor.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_con_canal_email_exige_email_y_no_movil()
    {
        var conductor = Conductor.Crear(
            Empresa, "C02", "Ana",
            Canal.Create("email")!, movil: null, email: Email.Create("ana@ejemplo.com"));

        conductor.Canal.Valor.Should().Be("email");
        conductor.Email!.Valor.Should().Be("ana@ejemplo.com");
        conductor.Movil.Should().BeNull();
    }

    [Fact]
    public void Crear_con_canal_sms_sin_movil_lanza()
    {
        var act = () => Conductor.Crear(
            Empresa, "C03", "Sin movil", Canal.Create("sms")!, movil: null, email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_con_canal_email_sin_email_lanza()
    {
        var act = () => Conductor.Crear(
            Empresa, "C04", "Sin email", Canal.Create("email")!, movil: Movil.Create("600000001"), email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_trima_codigo_nombre_y_campos_opcionales()
    {
        var conductor = Conductor.Crear(
            "  001  ", "  C05  ", "  Pepe  ",
            Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            taxId: "  12345678Z  ", licensePlate: "  1234ABC  ");

        conductor.Empresa.Should().Be("001");
        conductor.Codigo.Should().Be("C05");
        conductor.Nombre.Should().Be("Pepe");
        conductor.TaxId.Should().Be("12345678Z");
        conductor.LicensePlate.Should().Be("1234ABC");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_con_codigo_vacio_lanza(string codigo)
    {
        var act = () => Conductor.Crear(
            Empresa, codigo, "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_con_empresa_vacia_lanza(string empresa)
    {
        var act = () => Conductor.Crear(
            empresa, "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activar_y_desactivar_cambian_el_estado()
    {
        var conductor = Conductor.Crear(
            Empresa, "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null);

        conductor.Activo.Should().BeTrue();
        conductor.Desactivar();
        conductor.Activo.Should().BeFalse();
        conductor.Activar();
        conductor.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_con_language_vacio_cae_a_es_por_defecto()
    {
        var conductor = Conductor.Crear(
            Empresa, "C01", "Pepe", Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            language: "   ");

        conductor.Language.Should().Be("es");
    }
}