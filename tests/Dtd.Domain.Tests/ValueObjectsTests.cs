using System.Reflection;
using Dtd.Domain.Documentos.ValueObjects;
using FluentAssertions;

namespace Dtd.Domain.Tests;

public class ValueObjectsTests
{
    [Fact]
    public void RangoFechas_con_desde_menor_que_hasta_se_crea_correctamente()
    {
        var rango = RangoFechas.Create(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 5));

        rango.FechaDesde.Should().Be(new DateOnly(2026, 7, 1));
        rango.FechaHasta.Should().Be(new DateOnly(2026, 7, 5));
        rango.Contiene(new DateOnly(2026, 7, 3)).Should().BeTrue();
        rango.Contiene(new DateOnly(2026, 6, 30)).Should().BeFalse();
    }

    [Fact]
    public void RangoFechas_con_desde_posterior_a_hasta_lanza()
    {
        var act = () => RangoFechas.Create(new DateOnly(2026, 7, 5), new DateOnly(2026, 7, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("600000001", "600000001")]
    [InlineData("+34 600 000 002", "34600000002")]
    public void Movil_normaliza_a_digitos(string raw, string expected)
    {
        var movil = Movil.Create(raw);
        movil!.Valor.Should().Be(expected);
    }

    [Fact]
    public void Movil_vacio_o_nulo_devuelve_null()
    {
        Movil.Create(null).Should().BeNull();
        Movil.Create("   ").Should().BeNull();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    public void Movil_con_longitud_invalida_lanza(string raw)
    {
        var act = () => Movil.Create(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("email")]
    [InlineData("sms")]
    [InlineData("whatsapp")]
    [InlineData(" EMAIL ")]
    public void Canal_create_acepta_los_valores_admitidos(string raw)
    {
        var canal = Canal.Create(raw);

        canal.Should().NotBeNull();
        canal!.Valor.Should().Be(raw.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("telegram")]
    [InlineData("fax")]
    public void Canal_create_rechaza_valores_no_admitidos(string raw)
    {
        var act = () => Canal.Create(raw);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Canal_vacio_o_nulo_devuelve_null()
    {
        Canal.Create(null).Should().BeNull();
        Canal.Create("   ").Should().BeNull();
    }

    [Fact]
    public void Canal_requiere_movil_para_sms_y_whatsapp_y_email_para_email()
    {
        Canal.Create("sms")!.RequiereMovil.Should().BeTrue();
        Canal.Create("whatsapp")!.RequiereMovil.Should().BeTrue();
        Canal.Create("sms")!.RequiereEmail.Should().BeFalse();

        Canal.Create("email")!.RequiereEmail.Should().BeTrue();
        Canal.Create("email")!.RequiereMovil.Should().BeFalse();
    }

    [Theory]
    [InlineData("SMS", true, false)]
    [InlineData("WhatsApp", true, false)]
    [InlineData("EMAIL", false, true)]
    public void Canal_constructor_normaliza_a_minusculas_como_ef_al_cargar_de_bd(string raw, bool requiereMovil, bool requiereEmail)
    {
        // EF Core reconstruye el VO Canal desde la columna `channel` usando el constructor privado
        // (saltándose Create). Un seed SQL manual puede guardar "SMS"/"WhatsApp" en mayúsculas;
        // si el constructor no normaliza, RequiereMovil/RequiereEmail comparan Valor == "sms"/"email"
        // y devuelven false → ConductorAsignado.TieneCanalValido false → Documento.ConductorSinCanal.
        var canal = (Canal)Activator.CreateInstance(typeof(Canal), BindingFlags.Instance | BindingFlags.NonPublic, null, [raw], null)!;

        canal.Valor.Should().Be(raw.ToLowerInvariant(), "el constructor debe normalizar como Create");
        canal.RequiereMovil.Should().Be(requiereMovil);
        canal.RequiereEmail.Should().Be(requiereEmail);
    }
}