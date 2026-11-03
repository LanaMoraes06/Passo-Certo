using PassoCerto.Web.Models.Entity;

namespace PassoCerto.Web.Repositories.Interfaces;

public interface IChecklistRepository
{
    
    Task<List<CategoriaChecklist>> ListarCategoriasAsync();

    Task<List<ItemChecklist>> ListarItensAtivosAsync();

    Task<List<ItemChecklist>> ListarPorCategoriaAsync(
        long categoriaId);

    Task<CategoriaChecklist?> BuscarPorIdAsync(long id);

    Task<long> CriarAsync(CategoriaChecklist categoria);

    Task<bool> AtualizarAsync(CategoriaChecklist categoria);

    Task<bool> DesativarAsync(long id);

    Task<bool> ReativarAsync(long id);

    Task<bool> ExisteNomeCategoriaAsync(
        string nome,
        long? ignorarId = null);

    Task<bool> PossuiItensAtivosAsync(long categoriaId);


    // Administração de itens
    Task<List<ItemChecklist>> ListarTodosItensAsync();
    Task<ItemChecklist?> BuscarItemPorIdAsync(long id);
    Task<long> CriarItemAsync(ItemChecklist item);
    Task<bool> AtualizarItemAsync(ItemChecklist item);
    Task<bool> DesativarItemAsync(long id);
    Task<bool> ReativarItemAsync(long id);
}
