using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Lojas;

[Authorize]
public class IndexModel(LojaService lojaService) : PageModel
{
    public List<Loja> Lojas { get; private set; } = new();

    public async Task OnGetAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        Lojas = await lojaService.ListarPermitidasAsync(usuarioId);
    }
}