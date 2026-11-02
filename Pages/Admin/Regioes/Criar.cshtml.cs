using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Regioes;

[Authorize(Policy = "SomenteAdministrador")]
public class CriarModel(RegiaoService regiaoService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe o nome da região.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da região")]
        public string Nome { get; set; } = string.Empty;
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
            var regiao = new Regiao
            {
                Nome = Input.Nome,
                Ativo = true
            };

            await regiaoService.CriarAsync(regiao);

            TempData["Sucesso"] = "Região cadastrada com sucesso.";

            return RedirectToPage("Index");
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("Input.Nome", ex.Message);
            return Page();
        }
    }
}
