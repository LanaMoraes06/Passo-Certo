using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Exceptions;

namespace PassoCerto.Web.Services;

public class RegiaoService(IRegiaoRepository regiaoRepository)
{
    public Task<List<Regiao>> ListarAtivasAsync()
        => regiaoRepository.ListarAtivasAsync();

    public async Task<Regiao> BuscarPorIdAsync(long regiaoId)
    {
        if (regiaoId <= 0)
            throw new BusinessException("Identificador inválido.");

        return await regiaoRepository.BuscarPorIdAsync(regiaoId)
            ?? throw new NotFoundException("Região não encontrada.");
    }

    public async Task<long> CriarAsync(Regiao regiao)
    {
        ArgumentNullException.ThrowIfNull(regiao);

        regiao.Nome = NormalizarNome(regiao.Nome);

        var regioesAtivas = await regiaoRepository.ListarAtivasAsync();
        if (regioesAtivas.Any(r => string.Equals(
            r.Nome, regiao.Nome, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessException(
                "Já existe uma região com esse nome.");

        return await regiaoRepository.CriarAsync(regiao);
    }

    public async Task AtualizarAsync(Regiao regiao)
    {
        ArgumentNullException.ThrowIfNull(regiao);

        if (regiao.Id <= 0)
            throw new BusinessException("Identificador inválido.");

        regiao.Nome = NormalizarNome(regiao.Nome);

        var existente = await regiaoRepository
            .BuscarPorIdAsync(regiao.Id);

        if (existente is null)
            throw new NotFoundException("Região não encontrada.");

        var regioesAtivas = await regiaoRepository.ListarAtivasAsync();
        if (regioesAtivas.Any(r => r.Id != regiao.Id && string.Equals(
            r.Nome, regiao.Nome, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException(
                "Já existe uma região com esse nome.");
        }

        regiao.AtualizadoEm = DateTime.UtcNow;

        bool atualizou = await regiaoRepository
            .AtualizarAsync(regiao);

        if (!atualizou)
            throw new NotFoundException(
                "Região não encontrada para atualização.");
    }

    public async Task DesativarAsync(long regiaoId)
    {
        if (regiaoId <= 0)
            throw new BusinessException("Identificador inválido.");

        var existente = await regiaoRepository
            .BuscarPorIdAsync(regiaoId);

        if (existente is null)
            throw new NotFoundException("Região não encontrada.");

        if (!existente.Ativo)
            throw new BusinessException(
                "A região já está desativada.");

        bool desativou = await regiaoRepository
            .DesativarAsync(regiaoId);

        if (!desativou)
            throw new NotFoundException(
                "Não foi possível desativar a região.");
    }

    private static string NormalizarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessException(
                "Informe o nome da região.");

        return nome.Trim();
    }
}
