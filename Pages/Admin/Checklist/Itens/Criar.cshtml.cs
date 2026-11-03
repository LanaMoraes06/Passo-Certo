using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.DTOs.Admin;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Checklist.Itens;

[Authorize(Policy = "SomenteAdministrador")]
public class CriarModel(
    ItemChecklistService itemService,
    CategoriaChecklistService categoriaService) : PageModel
{
    [BindProperty]
    public ItemChecklistInput Input { get; set; } = new();

    public List<SelectListItem> Categorias { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await CarregarCategoriasAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarCategoriasAsync();
            return Page();
        }

        try
        {
            await itemService.CriarAsync(new ItemChecklist
            {
                CategoriaId = Input.CategoriaId,
                Titulo = Input.Titulo,
                Descricao = Input.Descricao,
                Ordem = Input.Ordem
            });

            TempData["Sucesso"] = "Item cadastrado com sucesso.";
            return RedirectToPage("Index");
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (NotFoundException)
        {
            ModelState.AddModelError(
                "Input.CategoriaId",
                "A categoria selecionada não existe.");
        }

        await CarregarCategoriasAsync();
        return Page();
    }

    private async Task CarregarCategoriasAsync()
    {
        Categorias = (await categoriaService.ListarAsync())
            .Where(c => c.Ativo)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nome
            })
            .ToList();
    }
}
