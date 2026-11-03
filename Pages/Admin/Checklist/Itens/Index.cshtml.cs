using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Checklist.Itens;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel(
    ItemChecklistService itemService,
    CategoriaChecklistService categoriaService) : PageModel
{
    public List<ItemChecklist> Itens { get; private set; } = new();
    public Dictionary<long, string> Categorias { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Itens = await itemService.ListarAsync();

        Categorias = (await categoriaService.ListarAsync())
            .ToDictionary(c => c.Id, c => c.Nome);
    }

    public async Task<IActionResult> OnPostDesativarAsync(long id)
    {
        try
        {
            await itemService.DesativarAsync(id);
            TempData["Sucesso"] = "Item desativado com sucesso.";
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
            await itemService.ReativarAsync(id);
            TempData["Sucesso"] = "Item reativado com sucesso.";
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
