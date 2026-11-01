using System.Security.Claims;

namespace PassoCerto.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long ObterUsuarioId(
        this ClaimsPrincipal usuario)
    {
        string? id = usuario.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!long.TryParse(id, out long usuarioId))
        {
            throw new UnauthorizedAccessException(
                "Usuário não autenticado.");
        }

        return usuarioId;
    }
}