using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.DTOs.Admin;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Usuarios;

[Authorize(Policy = "SomenteAdministrador")]
public class EditarModel(UsuarioService usuarioService) : PageModel
{
    [BindProperty]
    public EditarUsuarioInput Input { get; set; } = new();

    [BindProperty]
    public string? NovaSenha { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        try
        {
            await CarregarUsuarioAsync(id);
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
            return Page();

        try
        {
            await usuarioService.AtualizarAsync(
                id,
                Input,
                User.ObterUsuarioId());

            TempData["Sucesso"] = "Usuário atualizado com sucesso.";

            return RedirectToPage("Index");
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

    public async Task<IActionResult> OnPostRedefinirSenhaAsync(long id)
    {
        if (string.IsNullOrWhiteSpace(NovaSenha) ||
            NovaSenha.Length < 8)
        {
            TempData["Erro"] =
                "A nova senha deve possuir pelo menos 8 caracteres.";

            return RedirectToPage(new { id });
        }

        try
        {
            await usuarioService.RedefinirSenhaAsync(
                id, NovaSenha);

            TempData["Sucesso"] = "Senha redefinida com sucesso.";

            return RedirectToPage(new { id });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    private async Task CarregarUsuarioAsync(long id)
    {
        var usuario = await usuarioService.BuscarPorIdAsync(id);

        Input = new EditarUsuarioInput
        {
            Login = usuario.Login,
            Perfil = usuario.Perfil
        };
    }
}
