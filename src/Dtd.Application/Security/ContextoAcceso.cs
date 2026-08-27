namespace Dtd.Application.Security
{

    public sealed record ContextoAcceso(
        string Empresa,
        IReadOnlyCollection<Guid> AlmacenesIds);
}