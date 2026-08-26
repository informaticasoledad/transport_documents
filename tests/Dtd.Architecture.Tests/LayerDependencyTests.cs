using System.Reflection;
using Dtd.Api.Modules;
using Dtd.Application;
using Dtd.Domain.Documentos;
using Dtd.Infrastructure;
using FluentAssertions;
using NetArchTest.Rules;

namespace Dtd.Architecture.Tests;

public class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(DocumentoDigitalTransporte).Assembly;
    private static readonly Assembly Application = typeof(Dtd.Application.DependencyInjection).Assembly;
    private static readonly Assembly Infrastructure = typeof(Dtd.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly Api = typeof(DocumentosModule).Assembly;

    [Fact]
    public void Domain_no_depende_de_capas_superiores_ni_de_infraestructura()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .NotHaveDependencyOn("Dtd.Application")
            .And().NotHaveDependencyOn("Dtd.Infrastructure")
            .And().NotHaveDependencyOn("Dtd.Api")
            .And().NotHaveDependencyOn("MediatR")
            .And().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_no_depende_de_infrastructure_ni_de_api()
    {
        var result = Types.InAssembly(Application)
            .Should()
            .NotHaveDependencyOn("Dtd.Infrastructure")
            .And().NotHaveDependencyOn("Dtd.Api")
            .And().NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_no_depende_de_api()
    {
        var result = Types.InAssembly(Infrastructure)
            .Should()
            .NotHaveDependencyOn("Dtd.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Api_puede_depender_de_todas_las_capas_inferiores()
    {
        // The Api assembly must reference both lower-layer assemblies.
        var referenced = Api.GetReferencedAssemblies().Select(a => a.Name).ToHashSet();

        referenced.Should().Contain("Dtd.Application");
        referenced.Should().Contain("Dtd.Infrastructure");
    }
}