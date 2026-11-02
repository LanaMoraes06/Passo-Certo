using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Lojas;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel(LojaService lojaService) : PageModel
{
    public List<Loja> Lojas { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Lojas = await lojaService.ListarAsync();
    }

    public async Task<IActionResult> OnPostDesativarAsync(long id)
    {
        try
        {
            await lojaService.DesativarAsync(id);
            TempData["Sucesso"] = "Loja desativada com sucesso.";
        }
        catch (BusinessException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        catch (NotFoundException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReativarAsync(long id)
    {
        try
        {
            await lojaService.ReativarAsync(id);
            TempData["Sucesso"] = "Loja reativada com sucesso.";
        }
        catch (BusinessException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        catch (NotFoundException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToPage();
    }
}
