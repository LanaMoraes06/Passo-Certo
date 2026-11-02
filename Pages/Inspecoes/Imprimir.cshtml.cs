using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Inspecoes;

[Authorize]
public class ImprimirModel(
    InspecaoService inspecaoService) : PageModel
{
    public Inspecao Inspecao { get; private set; } = null!;

    public List<RespostaInspecao> Itens { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(long id)
    {
        long usuarioId = User.ObterUsuarioId();

        try
        {
            Inspecao = await inspecaoService
                .BuscarPorIdAsync(id, usuarioId);

            Itens = await inspecaoService
                .ListarRespostasAsync(id, usuarioId);

            return Page();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }
}
