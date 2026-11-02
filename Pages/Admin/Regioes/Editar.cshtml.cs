
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Regioes;

[Authorize(Policy = "SomenteAdministrador")]
public class EditarModel(RegiaoService regiaoService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da região.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome da região")]
        public string Nome { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        try
        {
            var regiao = await regiaoService.BuscarPorIdAsync(id);

            Input = new InputModel
            {
                Id = regiao.Id,
                Nome = regiao.Nome
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
            var regiao = await regiaoService.BuscarPorIdAsync(id);

            regiao.Nome = Input.Nome;

            await regiaoService.AtualizarAsync(regiao);

            TempData["Sucesso"] = "Região atualizada com sucesso.";

            return RedirectToPage("Index");
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError("Input.Nome", ex.Message);
            return Page();
        }
    }
}
