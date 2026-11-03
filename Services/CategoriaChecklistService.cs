using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class CategoriaChecklistService(
    IChecklistRepository checklistRepository)
{
    public Task<List<CategoriaChecklist>> ListarAsync()
        => checklistRepository.ListarCategoriasAsync();

    public async Task<CategoriaChecklist> BuscarPorIdAsync(long id)
    {
        if (id <= 0)
            throw new BusinessException(
                "Identificador de categoria inválido.");

        return await checklistRepository.BuscarPorIdAsync(id)
            ?? throw new NotFoundException(
                "Categoria não encontrada.");
    }

    public async Task<long> CriarAsync(CategoriaChecklist categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        NormalizarEValidar(categoria);

        if (await checklistRepository.ExisteNomeCategoriaAsync(
            categoria.Nome))
        {
            throw new BusinessException(
                "Já existe uma categoria com esse nome.");
        }

        categoria.Ativo = true;

        return await checklistRepository.CriarAsync(categoria);
    }

    public async Task AtualizarAsync(CategoriaChecklist categoria)
    {
        ArgumentNullException.ThrowIfNull(categoria);

        if (categoria.Id <= 0)
            throw new BusinessException(
                "Identificador de categoria inválido.");

        NormalizarEValidar(categoria);

        var existente = await BuscarPorIdAsync(categoria.Id);

        if (await checklistRepository.ExisteNomeCategoriaAsync(
            categoria.Nome, categoria.Id))
        {
            throw new BusinessException(
                "Já existe uma categoria com esse nome.");
        }

        categoria.Ativo = existente.Ativo;

        bool atualizou = await checklistRepository
            .AtualizarAsync(categoria);

        if (!atualizou)
            throw new NotFoundException(
                "Categoria não encontrada para atualização.");
    }

    public async Task DesativarAsync(long id)
    {
        var categoria = await BuscarPorIdAsync(id);

        if (!categoria.Ativo)
            throw new BusinessException(
                "A categoria já está desativada.");

        if (await checklistRepository.PossuiItensAtivosAsync(id))
        {
            throw new BusinessException(
                "Não é possível desativar uma categoria " +
                "que possui itens ativos.");
        }

        bool desativou = await checklistRepository
            .DesativarAsync(id);

        if (!desativou)
            throw new BusinessException(
                "Não foi possível desativar a categoria.");
    }

    public async Task ReativarAsync(long id)
    {
        var categoria = await BuscarPorIdAsync(id);

        if (categoria.Ativo)
            throw new BusinessException(
                "A categoria já está ativa.");

        bool reativou = await checklistRepository
            .ReativarAsync(id);

        if (!reativou)
            throw new BusinessException(
                "Não foi possível reativar a categoria.");
    }

    private static void NormalizarEValidar(
        CategoriaChecklist categoria)
    {
        categoria.Nome = categoria.Nome?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(categoria.Nome))
            throw new BusinessException(
                "Informe o nome da categoria.");

        if (categoria.Ordem <= 0)
            throw new BusinessException(
                "A ordem deve ser maior que zero.");
    }
}
