namespace PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Models.Entity;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> BuscarPorLoginAsync(string login);
    Task<List<Usuario>> ListarRegioesPorUsuarioAsync(long usuarioId);
}