using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.DTOs.Admin;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class UsuarioService(IUsuarioRepository usuarioRepository)
{
    public Task<List<Usuario>> ListarAsync()
        => usuarioRepository.ListarAsync();

    public async Task<Usuario> BuscarPorIdAsync(long id)
    {
        if (id <= 0)
            throw new BusinessException("Usuário inválido.");

        return await usuarioRepository.BuscarPorIdAsync(id)
            ?? throw new NotFoundException("Usuário não encontrado.");
    }

    public async Task<long> CriarAsync(CriarUsuarioInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var login = NormalizarLogin(input.Login);
        ValidarPerfil(input.Perfil);

        if (string.IsNullOrWhiteSpace(input.Senha) ||
            input.Senha.Length < 8)
            throw new BusinessException(
                "A senha deve possuir pelo menos 8 caracteres.");

        if (await usuarioRepository.ExisteLoginAsync(login))
            throw new BusinessException(
                "Já existe um usuário com esse login.");

        var usuario = new Usuario
        {
            Login = login,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(input.Senha),
            Perfil = input.Perfil!.Value,
            Ativo = true
        };

        return await usuarioRepository.CriarAsync(usuario);
    }

    public async Task AtualizarAsync(
        long id,
        EditarUsuarioInput input,
        long administradorId)
    {
        ArgumentNullException.ThrowIfNull(input);

        var usuario = await BuscarPorIdAsync(id);
        var login = NormalizarLogin(input.Login);

        ValidarPerfil(input.Perfil);

        if (id == administradorId &&
            input.Perfil != PerfilUsuario.ADMINISTRADOR)
            throw new BusinessException(
                "Você não pode remover seu próprio perfil de administrador.");

        if (await usuarioRepository.ExisteLoginAsync(login, id))
            throw new BusinessException(
                "Já existe um usuário com esse login.");

        usuario.Login = login;
        usuario.Perfil = input.Perfil!.Value;

        if (!await usuarioRepository.AtualizarAsync(usuario))
            throw new NotFoundException(
                "Usuário não encontrado para atualização.");
    }

    public async Task DesativarAsync(
        long id,
        long administradorId)
    {
        if (id == administradorId)
            throw new BusinessException(
                "Você não pode desativar sua própria conta.");

        var usuario = await BuscarPorIdAsync(id);

        if (!usuario.Ativo)
            throw new BusinessException(
                "O usuário já está desativado.");

        if (!await usuarioRepository.DesativarAsync(id))
            throw new BusinessException(
                "Não foi possível desativar o usuário.");
    }

    public async Task ReativarAsync(long id)
    {
        var usuario = await BuscarPorIdAsync(id);

        if (usuario.Ativo)
            throw new BusinessException(
                "O usuário já está ativo.");

        if (!await usuarioRepository.ReativarAsync(id))
            throw new BusinessException(
                "Não foi possível reativar o usuário.");
    }

    public async Task RedefinirSenhaAsync(
        long id,
        string novaSenha)
    {
        await BuscarPorIdAsync(id);

        if (string.IsNullOrWhiteSpace(novaSenha) ||
            novaSenha.Length < 8)
            throw new BusinessException(
                "A nova senha deve possuir pelo menos 8 caracteres.");

        string hash = BCrypt.Net.BCrypt.HashPassword(novaSenha);

        if (!await usuarioRepository.AtualizarSenhaHashAsync(id, hash))
            throw new NotFoundException(
                "Usuário não encontrado para atualização da senha.");
    }

    private static string NormalizarLogin(string? login)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new BusinessException("Informe o login.");

        login = login.Trim().ToLowerInvariant();

        if (login.Length > 100)
            throw new BusinessException(
                "O login deve ter no máximo 100 caracteres.");

        return login;
    }

    private static void ValidarPerfil(PerfilUsuario? perfil)
    {
        if (!perfil.HasValue || !Enum.IsDefined(perfil.Value))
            throw new BusinessException("Selecione um perfil válido.");
    }
}
