using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.DTOs.Dashboard;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Dashboard;

[Authorize(Policy = "PodeConsultarRelatorios")]
public class IndexModel(
    DashboardService dashboardService,
    LojaService lojaService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public FiltroDashboard Filtro { get; set; } = new();

    public List<Loja> Lojas { get; private set; } = new();

    public List<ResumoRegiao> RegioesFiltro
        { get; private set; } = new();

    public ResumoDashboard Resumo { get; private set; } = new();

    public List<LojaNaoInspecionada> LojasNaoInspecionadas
        { get; private set; } = new();

    public List<ItemPendenteResumo> TopPendencias
        { get; private set; } = new();

    public List<ResumoRegiao> Regioes
        { get; private set; } = new();

    public string? Erro { get; private set; }

    public async Task OnGetAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        Lojas = await lojaService
            .ListarPermitidasAsync(usuarioId);

        RegioesFiltro = await dashboardService
            .ObterResumoPorRegiaoAsync(
                new FiltroDashboard(),
                usuarioId);

        if (Filtro.RegiaoId.HasValue)
        {
            Lojas = Lojas
                .Where(l => l.RegiaoId == Filtro.RegiaoId.Value)
                .ToList();
        }

        if (!ModelState.IsValid)
        {
            Erro = "Verifique os filtros informados.";
            return;
        }

        if (Filtro.LojaId.HasValue &&
            !Lojas.Any(l => l.Id == Filtro.LojaId.Value))
        {
            Erro = "Loja inválida para a região selecionada.";
            return;
        }

        try
        {
            Resumo = await dashboardService
                .ObterResumoAsync(Filtro, usuarioId);

            LojasNaoInspecionadas = await dashboardService
                .ObterLojasNaoInspecionadasAsync(
                    Filtro, usuarioId);

            TopPendencias = await dashboardService
                .ObterTopPendenciasAsync(
                    Filtro, usuarioId);

            Regioes = await dashboardService
                .ObterResumoPorRegiaoAsync(
                    Filtro, usuarioId);
        }
        catch (BusinessException ex)
        {
            Erro = ex.Message;
        }
    }
}
