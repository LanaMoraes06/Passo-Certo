using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Admin.Lojas;

[Authorize(Policy = "SomenteAdministrador")]
public class CriarModel(
    LojaService lojaService,
    RegiaoService regiaoService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Regioes { get; private set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe o código da loja.")]
        [StringLength(30)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o nome da loja.")]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o gerente.")]
        [Display(Name = "Nome do gerente")]
        [StringLength(150)]
        public string GerenteNome { get; set; } = string.Empty;

        [Range(1, long.MaxValue,
            ErrorMessage = "Selecione uma região.")]
        [Display(Name = "Região")]
        public long RegiaoId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await CarregarRegioesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarRegioesAsync();
            return Page();
        }

        try
        {
            var loja = new Loja
            {
                Codigo = Input.Codigo,
                Nome = Input.Nome,
                GerenteNome = Input.GerenteNome,
                RegiaoId = Input.RegiaoId,
                Ativo = true
            };

            await lojaService.CriarAsync(loja);

            TempData["Sucesso"] = "Loja cadastrada com sucesso.";

            return RedirectToPage("Index");
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await CarregarRegioesAsync();
            return Page();
        }
        catch (NotFoundException)
        {
            ModelState.AddModelError(
                "Input.RegiaoId",
                "A região selecionada não existe.");

            await CarregarRegioesAsync();
            return Page();
        }
    }

    private async Task CarregarRegioesAsync()
    {
        var regioes = await regiaoService.ListarAtivasAsync();

        Regioes = regioes
            .OrderBy(r => r.Nome)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nome
            })
            .ToList();
    }
}
