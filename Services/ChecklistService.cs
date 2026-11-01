using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Exceptions;

namespace PassoCerto.Web.Services;

public class ChecklistService(IChecklistRepository checklistRepository)
{
    public Task<List<CategoriaChecklist>> ListarCategoriasAsync()
        => checklistRepository.ListarCategoriasAsync();

    public Task<List<ItemChecklist>> ListarItensAtivosAsync()
        => checklistRepository.ListarItensAtivosAsync();

    public Task<List<ItemChecklist>> ListarPorCategoriaAsync(long categoriaId)
    {
        if (categoriaId <= 0)
            throw new BusinessException("Categoria inválida.");

        return checklistRepository.ListarPorCategoriaAsync(categoriaId);
    }
}