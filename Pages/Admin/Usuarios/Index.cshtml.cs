using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Usuarios;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel(UsuarioService usuarioService) : PageModel
{
    public List<Usuario> Usuarios { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Usuarios = await usuarioService.ListarAsync();
    }

    public async Task<IActionResult> OnPostDesativarAsync(long id)
    {
        try
        {
            await usuarioService.DesativarAsync(
                id, User.ObterUsuarioId());

            TempData["Sucesso"] = "Usuário desativado com sucesso.";
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
            await usuarioService.ReativarAsync(id);

            TempData["Sucesso"] = "Usuário reativado com sucesso.";
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
