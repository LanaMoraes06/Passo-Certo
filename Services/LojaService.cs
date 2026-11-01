using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Exceptions;

namespace PassoCerto.Web.Services;

public class LojaService(ILojaRepository lojaRepository)
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

        return await lojaRepository.BuscarParaInspecaoAsync(lojaId, usuarioId)
            ?? throw new NotFoundException(
                "Loja não encontrada ou indisponível para inspeção.");
    }
}