using PassoCerto.Web.Models.Entity;

namespace PassoCerto.Web.Repositories.Interfaces;

public interface IRepository<T>
{
    Task<List<T>> ListarAsync();

    Task<T?> BuscarPorIdAsync(long id);

    Task<long> CriarAsync(T entity);

    Task<bool> AtualizarAsync(T entity);

    Task<bool> DesativarAsync(long id);
}