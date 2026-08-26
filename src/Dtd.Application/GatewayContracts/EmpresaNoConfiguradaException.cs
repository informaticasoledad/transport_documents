namespace Dtd.Application.GatewayContracts;

/// <summary>
/// La lanza el gateway del ERP cuando una empresa no tiene endpoint configurado en <c>empresas</c>.
/// La capturan los handlers de aplicación y se eleva como <c>Error.Failure("Empresa.ErpNoConfigurado")</c>.
/// </summary>
public sealed class EmpresaNoConfiguradaException : Exception
{
    public string Empresa { get; }

    public EmpresaNoConfiguradaException(string empresa, string? message = null)
        : base(message ?? $"No hay endpoint ERP configurado para la empresa '{empresa}'.")
    {
        Empresa = empresa;
    }
}