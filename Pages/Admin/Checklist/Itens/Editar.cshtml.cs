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
public class EditarModel(
    ItemChecklistService itemService,
    CategoriaChecklistService categoriaService) : PageModel
{
    [BindProperty]
    public ItemChecklistInput Input { get; set; } = new();

    public List<SelectListItem> Categorias { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(long id)
    {
        try
        {
            var item = await itemService.BuscarPorIdAsync(id);

            Input = new ItemChecklistInput
            {
                CategoriaId = item.CategoriaId,
                Titulo = item.Titulo,
                Descricao = item.Descricao,
                Ordem = item.Ordem
            };

            await CarregarCategoriasAsync(item.CategoriaId);
            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException)
        {
            return BadRequest();
        }
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!ModelState.IsValid)
        {
            await CarregarCategoriasAsync(Input.CategoriaId);
            return Page();
        }

        try
        {
            var item = await itemService.BuscarPorIdAsync(id);

            item.CategoriaId = Input.CategoriaId;
            item.Titulo = Input.Titulo;
            item.Descricao = Input.Descricao;
            item.Ordem = Input.Ordem;

            await itemService.AtualizarAsync(item);

            TempData["Sucesso"] = "Item atualizado com sucesso.";
            return RedirectToPage("Index");
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await CarregarCategoriasAsync(Input.CategoriaId);
        return Page();
    }

    private async Task CarregarCategoriasAsync(long selecionada)
    {
        var categorias = await categoriaService.ListarAsync();

        Categorias = categorias
            .Where(c => c.Ativo || c.Id == selecionada)
            .OrderBy(c => c.Ordem)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Ativo
                    ? c.Nome
                    : $"{c.Nome} (inativa)",
                Disabled = !c.Ativo
            })
            .ToList();
    }
}
