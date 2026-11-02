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
public class EditarModel(
    LojaService lojaService,
    RegiaoService regiaoService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> Regioes { get; private set; } = new();

    public class InputModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Informe o código.")]
        [StringLength(30)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o nome.")]
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

    public async Task<IActionResult> OnGetAsync(long id)
    {
        try
        {
            var loja = await lojaService.BuscarPorIdAsync(id);

            Input = new InputModel
            {
                Id = loja.Id,
                Codigo = loja.Codigo,
                Nome = loja.Nome,
                GerenteNome = loja.GerenteNome,
                RegiaoId = loja.RegiaoId
            };

            await CarregarRegioesAsync(loja.RegiaoId);

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
        {
            await CarregarRegioesAsync(Input.RegiaoId);
            return Page();
        }

        try
        {
            var loja = await lojaService.BuscarPorIdAsync(id);

            loja.Codigo = Input.Codigo;
            loja.Nome = Input.Nome;
            loja.GerenteNome = Input.GerenteNome;
            loja.RegiaoId = Input.RegiaoId;

            await lojaService.AtualizarAsync(loja);

            TempData["Sucesso"] = "Loja atualizada com sucesso.";

            return RedirectToPage("Index");
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await CarregarRegioesAsync(Input.RegiaoId);
            return Page();
        }
    }

private async Task CarregarRegioesAsync(long regiaoSelecionada)
{
    var regioes = await regiaoService.ListarAtivasAsync();

    if (regiaoSelecionada > 0 &&
        regioes.All(r => r.Id != regiaoSelecionada))
    {
        try
        {
            var atual = await regiaoService
                .BuscarPorIdAsync(regiaoSelecionada);

            regioes.Add(atual);
        }
        catch (NotFoundException)
        {
        }
    }

    Regioes = regioes
        .OrderBy(r => r.Nome)
        .Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Ativo ? r.Nome : $"{r.Nome} (inativa)",
            Disabled = !r.Ativo
        })
        .ToList();
}
}
