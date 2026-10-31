using PassoCerto.Web.Models.DTOs.Auth;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class AuthService(IUsuarioRepository usuarioRepository)
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public async Task<UsuarioSessaoDto?> AutenticarAsync(
        LoginInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        string login = input.Login?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrEmpty(input.Senha))
        {
            return null;
        }

        var usuario = await _usuarioRepository
            .BuscarPorLoginAsync(login);

        if (usuario is null || !usuario.Ativo)
        {
            return null;
        }

        bool senhaValida = BCrypt.Net.BCrypt.Verify(
            input.Senha,
            usuario.SenhaHash
        );

        if (!senhaValida)
        {
            return null;
        }

        return new UsuarioSessaoDto
        {
            Id = usuario.Id,
            Login = usuario.Login,
            Perfil = usuario.Perfil
        };
    }
}
