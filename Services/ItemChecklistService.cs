using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class ItemChecklistService(
    IChecklistRepository repository,
    CategoriaChecklistService categoriaService)
{
    public Task<List<ItemChecklist>> ListarAsync()
        => repository.ListarTodosItensAsync();

    public async Task<ItemChecklist> BuscarPorIdAsync(long id)
    {
        if (id <= 0)
            throw new BusinessException("Identificador inválido.");

        return await repository.BuscarItemPorIdAsync(id)
            ?? throw new NotFoundException("Item não encontrado.");
    }

    public async Task<long> CriarAsync(ItemChecklist item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Validar(item);
        await ValidarCategoriaAsync(item.CategoriaId);

        item.Ativo = true;

        return await repository.CriarItemAsync(item);
    }

    public async Task AtualizarAsync(ItemChecklist item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Id <= 0)
            throw new BusinessException("Identificador inválido.");

        Validar(item);

        await BuscarPorIdAsync(item.Id);
        await ValidarCategoriaAsync(item.CategoriaId);

        if (!await repository.AtualizarItemAsync(item))
            throw new NotFoundException("Item não encontrado.");
    }

    public async Task DesativarAsync(long id)
    {
        var item = await BuscarPorIdAsync(id);

        if (!item.Ativo)
            throw new BusinessException("O item já está inativo.");

        if (!await repository.DesativarItemAsync(id))
            throw new BusinessException(
                "Não foi possível desativar o item.");
    }

    public async Task ReativarAsync(long id)
    {
        var item = await BuscarPorIdAsync(id);

        if (item.Ativo)
            throw new BusinessException("O item já está ativo.");

        await ValidarCategoriaAsync(item.CategoriaId);

        if (!await repository.ReativarItemAsync(id))
            throw new BusinessException(
                "Não foi possível reativar o item.");
    }

    private async Task ValidarCategoriaAsync(long categoriaId)
    {
        if (categoriaId <= 0)
            throw new BusinessException("Selecione uma categoria.");

        var categoria =
            await categoriaService.BuscarPorIdAsync(categoriaId);

        if (!categoria.Ativo)
            throw new BusinessException(
                "A categoria selecionada está inativa.");
    }

    private static void Validar(ItemChecklist item)
    {
        item.Titulo = item.Titulo?.Trim() ?? "";
        item.Descricao = item.Descricao?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(item.Titulo))
            throw new BusinessException(
                "Informe o título do item.");

        if (item.Ordem <= 0)
            throw new BusinessException(
                "A ordem deve ser maior que zero.");
    }
}
