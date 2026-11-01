using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.DTOs.Inspecoes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Inspecoes;

[Authorize(Policy = "PodeInspecionar")]
public class CriarModel(
    InspecaoService inspecaoService,
    LojaService lojaService) : PageModel
{
    [BindProperty]
    public CriarInspecaoInput Input { get; set; } = new();

    public List<Loja> Lojas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        Lojas = await lojaService.ListarPermitidasAsync(usuarioId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        if (!ModelState.IsValid)
        {
            await CarregarLojasAsync(usuarioId);
            return Page();
        }

        try
        {
            long inspecaoId = await inspecaoService
                .CriarAsync(Input, usuarioId);

            TempData["Sucesso"] =
                "Inspeção criada com sucesso!";

            return RedirectToPage(
                 "/Inspecoes/Detalhes",
                new { id = inspecaoId });
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);
        }
        catch (NotFoundException)
        {
            ModelState.AddModelError(
                string.Empty,
                "A loja selecionada não está disponível.");
        }

        await CarregarLojasAsync(usuarioId);

        return Page();
    }

    private async Task CarregarLojasAsync(long usuarioId)
    {
        Lojas = await lojaService.ListarPermitidasAsync(usuarioId);
    }
}
