using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Regioes;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel(RegiaoService regiaoService) : PageModel
{
    public List<Regiao> Regioes { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Regioes = await regiaoService.ListarAtivasAsync();
    }

    public async Task<IActionResult> OnPostDesativarAsync(long id)
    {
        try
        {
            await regiaoService.DesativarAsync(id);

            TempData["Sucesso"] = "Região desativada com sucesso.";
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
