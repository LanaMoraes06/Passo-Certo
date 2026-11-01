using PassoCerto.Web.Models.DTOs.Inspecoes;
using PassoCerto.Web.Models.Entity;

namespace PassoCerto.Web.Repositories.Interfaces;

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

    Task<(List<InspecaoResumoDto> Itens, long Total)>
    BuscarHistoricoAsync(FiltroHistorico filtro, long usuarioId);

    Task<List<Inspecao>> ListarPorLojaAsync(long lojaId, long usuarioId);
    
    Task<List<RespostaInspecao>> ListarRespostasAsync(long inspecaoId, long usuarioId);
}