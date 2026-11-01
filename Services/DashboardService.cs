using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.DTOs.Dashboard;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class DashboardService(
    IDashboardRepository dashboardRepository)
{
    public Task<ResumoDashboard> ObterResumoAsync(
        FiltroDashboard filtro, long usuarioId)
    {
        ValidarFiltros(filtro, usuarioId);

        return dashboardRepository.ObterResumoAsync(
            filtro, usuarioId);
    }

    public Task<List<LojaNaoInspecionada>>
        ObterLojasNaoInspecionadasAsync(
            FiltroDashboard filtro, long usuarioId)
    {
        ValidarFiltros(filtro, usuarioId);

        return dashboardRepository
            .ObterLojasNaoInspecionadasAsync(filtro, usuarioId);
    }

    public Task<List<ItemPendenteResumo>>
        ObterTopPendenciasAsync(
            FiltroDashboard filtro, long usuarioId)
    {
        ValidarFiltros(filtro, usuarioId);

        return dashboardRepository.ObterTopPendenciasAsync(
            filtro, usuarioId);
    }

    public Task<List<ResumoRegiao>>
        ObterResumoPorRegiaoAsync(
            FiltroDashboard filtro, long usuarioId)
    {
        ValidarFiltros(filtro, usuarioId);

        return dashboardRepository.ObterResumoPorRegiaoAsync(
            filtro, usuarioId);
    }

    private static void ValidarFiltros(
        FiltroDashboard filtro, long usuarioId)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        if (usuarioId <= 0)
            throw new BusinessException("Usuário inválido.");

        if (filtro.DataInicio.HasValue &&
            filtro.DataFim.HasValue &&
            filtro.DataInicio.Value > filtro.DataFim.Value)
        {
            throw new BusinessException(
                "A data inicial não pode ser posterior à data final.");
        }

        if (filtro.RegiaoId.HasValue && filtro.RegiaoId.Value <= 0)
            throw new BusinessException("Região inválida.");

        if (filtro.LojaId.HasValue && filtro.LojaId.Value <= 0)
            throw new BusinessException("Loja inválida.");
    }
}
