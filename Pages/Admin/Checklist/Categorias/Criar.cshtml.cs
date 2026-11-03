using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Checklist.Categorias;

[Authorize(Policy = "SomenteAdministrador")]
public class CriarModel(
    CategoriaChecklistService categoriaService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(100,
            ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da categoria")]
        public string Nome { get; set; } = string.Empty;

        [Range(1, int.MaxValue,
            ErrorMessage = "A ordem deve ser maior que zero.")]
        public int Ordem { get; set; } = 1;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var categoria = new CategoriaChecklist
            {
                Nome = Input.Nome,
                Ordem = Input.Ordem,
                Ativo = true
            };

            await categoriaService.CriarAsync(categoria);

            TempData["Sucesso"] =
                "Categoria cadastrada com sucesso.";

            return RedirectToPage("/Admin/Checklist/Index");
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
