using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Vinculos;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel(
    UsuarioService usuarioService,
    RegiaoService regiaoService,
    UsuarioRegiaoService usuarioRegiaoService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long? UsuarioId { get; set; }

    [BindProperty]
    public List<long> RegioesSelecionadas { get; set; } = new();

    public List<Usuario> Supervisores { get; private set; } = new();

    public List<Regiao> Regioes { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await CarregarOpcoesAsync();

        if (!Supervisores.Any())
            return Page();

        UsuarioId ??= Supervisores[0].Id; //ele pega o primeiro supervisor caso não tenha sido selecionado 

        if (!Supervisores.Any(u => u.Id == UsuarioId))
            return BadRequest();

        RegioesSelecionadas =
            await usuarioRegiaoService.ListarRegioesAsync(
                UsuarioId.Value);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CarregarOpcoesAsync();

        if (!UsuarioId.HasValue ||
            !Supervisores.Any(u => u.Id == UsuarioId.Value))
        {
            ModelState.AddModelError(
                string.Empty,
                "Selecione um supervisor válido.");

            return Page();
        }

        try
        {
            await usuarioRegiaoService.SubstituirRegioesAsync(
                UsuarioId.Value,
                RegioesSelecionadas ?? new List<long>());

            TempData["Sucesso"] =
                "Vínculos regionais atualizados com sucesso.";

            return RedirectToPage(new
            {
                usuarioId = UsuarioId.Value
            });
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);

            return Page();
        }
        catch (NotFoundException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);

            return Page();
        }
    }

    private async Task CarregarOpcoesAsync()
    {
        var usuarios = await usuarioService.ListarAsync();

        Supervisores = usuarios
            .Where(u =>
                u.Ativo &&
                u.Perfil == PerfilUsuario.SUPERVISOR)
            .OrderBy(u => u.Login)
            .ToList();

        Regioes = (await regiaoService.ListarAtivasAsync())
            .OrderBy(r => r.Nome)
            .ToList();
    }
}