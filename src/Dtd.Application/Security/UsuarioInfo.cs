namespace Dtd.Application.Security;

/// <summary>
/// Usuario autenticado: identidad del token (<c>Sub</c> y nombre a efectos de auditoría) y
/// empresas autorizadas. <see cref="Empresas"/> nunca es null
/// (vacío si el token no trae el claim de empresas).
/// </summary>
public sealed record UsuarioInfo(string Sub, string? Nombre, IReadOnlySet<string> Empresas);
