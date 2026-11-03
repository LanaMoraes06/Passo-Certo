
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Checklist;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel(
    CategoriaChecklistService categoriaService) : PageModel
{
    public List<CategoriaChecklist> Categorias { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Categorias = await categoriaService.ListarAsync();
    }

    public async Task<IActionResult> OnPostDesativarAsync(long id)
    {
        try
        {
            await categoriaService.DesativarAsync(id);
            TempData["Sucesso"] = "Categoria desativada com sucesso.";
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
            await categoriaService.ReativarAsync(id);
            TempData["Sucesso"] = "Categoria reativada com sucesso.";
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
