using PassoCerto.Web.Models.Entity;

namespace PassoCerto.Web.Repositories.Interfaces;

public interface IChecklistRepository
{
    Task<List<CategoriaChecklist>> ListarCategoriasAsync();

    Task<List<ItemChecklist>> ListarItensAtivosAsync();

    Task<List<ItemChecklist>> ListarPorCategoriaAsync(
        long categoriaId);
}