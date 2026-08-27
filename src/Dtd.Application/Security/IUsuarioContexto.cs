namespace Dtd.Application.Security;


public interface IUsuarioContexto
{
    UsuarioActual? Current { get; }
}
