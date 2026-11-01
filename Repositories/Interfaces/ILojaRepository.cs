namespace PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Models.Entity;

public interface ILojaRepository : IRepository<Loja>
{
    Task<List<Loja>> ListarPorRegiaoAsync(long regiaoId);

    Task<List<Loja>> ListarPermitidasAsync(long usuarioId);
}