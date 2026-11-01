using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.DTOs.Inspecoes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Inspecoes;

[Authorize]
public class IndexModel(
    InspecaoService inspecaoService,
    LojaService lojaService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public FiltroHistorico Filtro { get; set; } = new();

    public List<InspecaoResumoDto> Inspecoes
        { get; private set; } = new();

    public List<Loja> Lojas
        { get; private set; } = new();

    public long TotalRegistros { get; private set; }

    public int TotalPaginas =>
        (int)Math.Ceiling(TotalRegistros / 10.0);

    public string? Erro { get; private set; }

    public async Task OnGetAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        Lojas = await lojaService
            .ListarPermitidasAsync(usuarioId);

        if (!ModelState.IsValid)
        {
            Erro = "Verifique os filtros informados.";
            return;
        }

        try
        {
            var resultado = await inspecaoService
                .BuscarHistoricoAsync(Filtro, usuarioId);

            Inspecoes = resultado.Itens;
            TotalRegistros = resultado.Total;
        }
        catch (BusinessException ex)
        {
            Erro = ex.Message;
        }
    }
}
