using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Inspecoes;

[Authorize]
public class IndexModel(
    InspecaoService inspecaoService) : PageModel
{
    public List<Inspecao> Inspecoes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        Inspecoes = await inspecaoService.ListarAsync(usuarioId);
    }
}