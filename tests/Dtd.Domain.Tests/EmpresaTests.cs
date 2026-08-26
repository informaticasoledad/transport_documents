using Dtd.Domain.Empresas;
using FluentAssertions;

namespace Dtd.Domain.Tests;

public class EmpresaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EsValida_false_para_valores_vacios(string? valor)
    {
        Empresa.EsValida(valor).Should().BeFalse();
    }

    [Theory]
    [InlineData("1")]
    [InlineData("001")]
    [InlineData("1000")]
    [InlineData("ABC")]
    public void EsValida_true_para_strings_no_vacios(string valor)
    {
        Empresa.EsValida(valor).Should().BeTrue();
    }
}
