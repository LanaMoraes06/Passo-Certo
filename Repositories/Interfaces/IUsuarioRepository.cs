namespace PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Models.Entity;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> BuscarPorLoginAsync(string login);
    Task<List<long>> ListarRegioesPorUsuarioAsync(long usuarioId);
    Task<bool> ExisteLoginAsync(
    string login,
    long? ignorarId = null);

    Task<bool> ReativarAsync(long id);

    Task<bool> AtualizarSenhaHashAsync(
    long id,
    string novoHash);
}