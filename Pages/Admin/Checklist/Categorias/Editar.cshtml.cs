
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Checklist.Categorias;

[Authorize(Policy = "SomenteAdministrador")]
public class EditarModel(
    CategoriaChecklistService categoriaService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(100,
            ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da categoria")]
        public string Nome { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "A ordem deve ser maior que zero.")]
        public int Ordem { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        try
        {
            var categoria =
                await categoriaService.BuscarPorIdAsync(id);

            Input = new InputModel
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Ordem = categoria.Ordem
            };

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
        if (id != Input.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var categoria =
                await categoriaService.BuscarPorIdAsync(id);

            categoria.Nome = Input.Nome;
            categoria.Ordem = Input.Ordem;

            await categoriaService.AtualizarAsync(categoria);

            TempData["Sucesso"] =
                "Categoria atualizada com sucesso.";

            return RedirectToPage("/Admin/Checklist/Index");
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
