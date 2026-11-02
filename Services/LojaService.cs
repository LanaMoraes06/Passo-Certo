using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Exceptions;

namespace PassoCerto.Web.Services;

public class LojaService(
    ILojaRepository lojaRepository,
    RegiaoService regiaoService)
{
    public async Task<List<Loja>> ListarPermitidasAsync(long usuarioId)
    {
        if (usuarioId <= 0)
            throw new BusinessException("Usuário inválido.");

        return await lojaRepository.ListarPermitidasAsync(usuarioId);
    }

    public async Task<Loja> BuscarParaInspecaoAsync(
        long lojaId, long usuarioId)
    {
        if (lojaId <= 0 || usuarioId <= 0)
            throw new BusinessException("Identificadores inválidos.");

        return await lojaRepository.BuscarParaInspecaoAsync(
            lojaId, usuarioId)
            ?? throw new NotFoundException(
                "Loja não encontrada ou indisponível para inspeção.");
    }

    public async Task<long> CriarAsync(Loja loja)
    {
        ArgumentNullException.ThrowIfNull(loja);

        NormalizarEValidar(loja);

        await ValidarRegiaoAsync(loja.RegiaoId);

        if (await lojaRepository.ExisteCodigoAsync(loja.Codigo))
            throw new BusinessException(
                "Já existe uma loja com esse código.");

        loja.Ativo = true;

        return await lojaRepository.CriarAsync(loja);
    }

    public async Task AtualizarAsync(Loja loja)
    {
        ArgumentNullException.ThrowIfNull(loja);

        if (loja.Id <= 0)
            throw new BusinessException("Identificador inválido.");

        NormalizarEValidar(loja);

        var existente = await lojaRepository.BuscarPorIdAsync(loja.Id)
            ?? throw new NotFoundException("Loja não encontrada.");

        await ValidarRegiaoAsync(loja.RegiaoId);

        if (await lojaRepository.ExisteCodigoAsync(
            loja.Codigo, loja.Id))
        {
            throw new BusinessException(
                "Já existe uma loja com esse código.");
        }

        loja.Ativo = existente.Ativo;
        loja.CriadoEm = existente.CriadoEm;

        bool atualizou = await lojaRepository.AtualizarAsync(loja);

        if (!atualizou)
            throw new NotFoundException(
                "Loja não encontrada para atualização.");
    }

    public async Task DesativarAsync(long lojaId)
    {
        var loja = await BuscarPorIdAsync(lojaId);

        if (!loja.Ativo)
            throw new BusinessException(
                "A loja já está desativada.");

        bool desativou = await lojaRepository.DesativarAsync(lojaId);

        if (!desativou)
            throw new NotFoundException(
                "Não foi possível desativar a loja.");
    }

    public async Task ReativarAsync(long lojaId)
    {
        var loja = await BuscarPorIdAsync(lojaId);

        if (loja.Ativo)
            throw new BusinessException(
                "A loja já está ativa.");

        await ValidarRegiaoAsync(loja.RegiaoId);

        bool reativou = await lojaRepository.ReativarAsync(lojaId);

        if (!reativou)
            throw new NotFoundException(
                "Não foi possível reativar a loja.");
    }

    public async Task<Loja> BuscarPorIdAsync(long lojaId)
    {
        if (lojaId <= 0)
            throw new BusinessException("Identificador inválido.");

        return await lojaRepository.BuscarPorIdAsync(lojaId)
            ?? throw new NotFoundException("Loja não encontrada.");
    }

    private async Task ValidarRegiaoAsync(long regiaoId)
    {
        if (regiaoId <= 0)
            throw new BusinessException(
                "A região da loja é obrigatória.");

        var regiao = await regiaoService.BuscarPorIdAsync(regiaoId);

        if (!regiao.Ativo)
            throw new BusinessException(
                "Não é possível vincular uma loja a uma região inativa.");
    }

    private static void NormalizarEValidar(Loja loja)
    {
        loja.Nome = loja.Nome?.Trim() ?? string.Empty;
        loja.Codigo = loja.Codigo?.Trim() ?? string.Empty;
        loja.GerenteNome = loja.GerenteNome?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(loja.Nome))
            throw new BusinessException(
                "O nome da loja é obrigatório.");

        if (string.IsNullOrWhiteSpace(loja.Codigo))
            throw new BusinessException(
                "O código da loja é obrigatório.");

        if (string.IsNullOrWhiteSpace(loja.GerenteNome))
            throw new BusinessException(
                "O nome do gerente é obrigatório.");

        if (loja.RegiaoId <= 0)
            throw new BusinessException(
                "A região da loja é obrigatória.");
    }
    
    public Task<List<Loja>> ListarAsync()
    => lojaRepository.ListarAsync();
}
