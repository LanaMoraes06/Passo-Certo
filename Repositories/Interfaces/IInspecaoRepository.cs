namespace PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Models.Entity;

public interface IInspecaoRepository
{
    Task<List<Inspecao>> ListarAsync(long usuarioId);

    Task<Inspecao?> BuscarPorIdAsync(
        long id, long usuarioId);

    Task<long> CriarAsync(Inspecao inspecao);

    Task<bool> AtualizarAsync(
        Inspecao inspecao, long usuarioId);

    Task<bool> SalvarRascunhoAsync(
        long inspecaoId,
        List<RespostaInspecao> respostas,
        long usuarioId);

    Task<bool> ConcluirAsync(
        long inspecaoId, long usuarioId);
}