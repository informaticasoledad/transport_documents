using Dtd.Domain.Consignees;
using Dtd.Domain.Documentos.ValueObjects;
using FluentAssertions;

namespace Dtd.Domain.Tests;

public class ConsigneeTests
{
    private const string Empresa = "001";

    [Fact]
    public void Crear_con_canal_sms_y_movil_se_crea_activo()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS01", "Destinatario Uno",
            Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            taxId: "B87654321");

        consignee.Id.Should().NotBeEmpty();
        consignee.Empresa.Should().Be("001");
        consignee.Codigo.Should().Be("CS01");
        consignee.Nombre.Should().Be("Destinatario Uno");
        consignee.TaxId.Should().Be("B87654321");
        consignee.Canal.Valor.Should().Be("sms");
        consignee.Movil!.Valor.Should().Be("600000001");
        consignee.Email.Should().BeNull();
        consignee.Language.Should().Be("es");
        consignee.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_con_canal_email_exige_email_y_no_movil()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS02", "Destinatario Dos",
            Canal.Create("email")!, movil: null, email: Email.Create("dest@ejemplo.com"));

        consignee.Canal.Valor.Should().Be("email");
        consignee.Email!.Valor.Should().Be("dest@ejemplo.com");
        consignee.Movil.Should().BeNull();
    }

    [Fact]
    public void Crear_con_canal_sms_sin_movil_lanza()
    {
        var act = () => Consignee.Crear(
            Empresa, "CS03", "Sin movil", Canal.Create("sms")!, movil: null, email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_con_canal_email_sin_email_lanza()
    {
        var act = () => Consignee.Crear(
            Empresa, "CS04", "Sin email", Canal.Create("email")!, movil: Movil.Create("600000001"), email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_trima_codigo_nombre_y_campos_opcionales()
    {
        var consignee = Consignee.Crear(
            "  001  ", "  CS05  ", "  Destinatario  ",
            Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            taxId: "  B87654321  ");

        consignee.Empresa.Should().Be("001");
        consignee.Codigo.Should().Be("CS05");
        consignee.Nombre.Should().Be("Destinatario");
        consignee.TaxId.Should().Be("B87654321");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_con_codigo_vacio_lanza(string codigo)
    {
        var act = () => Consignee.Crear(
            Empresa, codigo, "Dest", Canal.Create("sms")!, Movil.Create("600000001"), email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_con_empresa_vacia_lanza(string empresa)
    {
        var act = () => Consignee.Crear(
            empresa, "CS01", "Dest", Canal.Create("sms")!, Movil.Create("600000001"), email: null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activar_y_desactivar_cambian_el_estado()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS01", "Dest", Canal.Create("sms")!, Movil.Create("600000001"), email: null);

        consignee.Activo.Should().BeTrue();
        consignee.Desactivar();
        consignee.Activo.Should().BeFalse();
        consignee.Activar();
        consignee.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_con_language_vacio_cae_a_es_por_defecto()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS01", "Dest", Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            language: "   ");

        consignee.Language.Should().Be("es");
    }

    [Fact]
    public void Actualizar_muta_campos_revalida_invariante_y_no_toca_empresa_ni_codigo()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS01", "Destinatario Viejo",
            Canal.Create("sms")!, Movil.Create("600000001"), email: null,
            taxId: "VIEJO");

        consignee.Actualizar(
            "Destinatario Nuevo", "NUEVO", Canal.Create("email")!, movil: null,
            email: Email.Create("dest@nuevo.com"), language: "en");

        consignee.Nombre.Should().Be("Destinatario Nuevo");
        consignee.TaxId.Should().Be("NUEVO");
        consignee.Canal.Valor.Should().Be("email");
        consignee.Email!.Valor.Should().Be("dest@nuevo.com");
        consignee.Movil.Should().BeNull();
        consignee.Language.Should().Be("en");
        // Empresa y Codigo son inmutables.
        consignee.Empresa.Should().Be("001");
        consignee.Codigo.Should().Be("CS01");
        // Actualizar no cambia Activo.
        consignee.Activo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_con_canal_sms_sin_movil_lanza()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS01", "Dest", Canal.Create("email")!, movil: null, email: Email.Create("dest@x.com"));

        var act = () => consignee.Actualizar(
            "Dest", null, Canal.Create("sms")!, movil: null, email: null, language: "es");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_con_nombre_vacio_lanza()
    {
        var consignee = Consignee.Crear(
            Empresa, "CS01", "Dest", Canal.Create("sms")!, Movil.Create("600000001"), email: null);

        var act = () => consignee.Actualizar(
            "   ", null, Canal.Create("sms")!, Movil.Create("600000001"), email: null, language: "es");
        act.Should().Throw<ArgumentException>();
    }
}