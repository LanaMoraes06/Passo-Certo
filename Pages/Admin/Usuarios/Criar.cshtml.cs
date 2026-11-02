using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.DTOs.Admin;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Usuarios;

[Authorize(Policy = "SomenteAdministrador")]
public class CriarModel(UsuarioService usuarioService) : PageModel
{
    [BindProperty]
    public CriarUsuarioInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await usuarioService.CriarAsync(Input);

            TempData["Sucesso"] = "Usuário cadastrado com sucesso.";

            return RedirectToPage("Index");
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
