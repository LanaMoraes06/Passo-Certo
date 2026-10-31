namespace PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Models.Entity;

public interface IRegiaoRepository : IRepository<Regiao>
{
    Task<List<Regiao>> ListarAtivasAsync();
}