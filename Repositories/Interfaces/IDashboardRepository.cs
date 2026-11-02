using PassoCerto.Web.Models.DTOs.Dashboard;

namespace PassoCerto.Web.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<ResumoDashboard> ObterResumoAsync(
        FiltroDashboard filtro, long usuarioId);

    Task<List<LojaNaoInspecionada>> ObterLojasNaoInspecionadasAsync(
        FiltroDashboard filtro, long usuarioId);

    Task<List<ItemPendenteResumo>> ObterTopPendenciasAsync(
        FiltroDashboard filtro, long usuarioId);

    Task<List<ResumoRegiao>> ObterResumoPorRegiaoAsync(
        FiltroDashboard filtro, long usuarioId);

    Task<List<EvolucaoMensalDto>> ObterEvolucaoMensalAsync(
    FiltroDashboard filtro,
    long usuarioId);
}