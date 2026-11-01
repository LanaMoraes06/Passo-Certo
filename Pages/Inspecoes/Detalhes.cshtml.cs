using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.DTOs.Inspecoes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Inspecoes;

[Authorize]
public class DetalhesModel(
    InspecaoService inspecaoService) : PageModel
{
    public Inspecao Inspecao { get; private set; } = null!;

    [BindProperty]
    public SalvarRascunhoInput Input { get; set; } = new();

    public bool PodeEditar =>
    User.IsInRole("SUPERVISOR") &&
    Inspecao.AutorId == User.ObterUsuarioId() &&
    Inspecao.Status == StatusInspecao.RASCUNHO;

    public async Task<IActionResult> OnGetAsync(long id)
    {
        return await CarregarAsync(id);
    }

    public async Task<IActionResult> OnPostSalvarAsync(long id)
    {
        long usuarioId = User.ObterUsuarioId();

        try
        {
            await inspecaoService.SalvarRascunhoAsync(
                id, Input, usuarioId);

            TempData["Sucesso"] =
                "Rascunho salvo com sucesso!";

            return RedirectToPage(new { id });
        }
        catch (BusinessException ex)
        {
            var resultado = await CarregarAsync(
                id,
                preservarInput: true);

            if (resultado is not PageResult)
                return resultado;

            AdicionarErrosNosCampos(ex.Message);

            return Page();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }


    public async Task<IActionResult> OnPostConcluirAsync(long id)
    {
        long usuarioId = User.ObterUsuarioId();

        try
        {
            await inspecaoService.SalvarRascunhoAsync(
                id, Input, usuarioId);

            await inspecaoService.ConcluirAsync(
                id, usuarioId);

            TempData["Sucesso"] =
                "Inspeção concluída com sucesso!";

            return RedirectToPage("/Inspecoes/Index");
        }
        catch (BusinessException ex)
        {
            var resultado = await CarregarAsync(
                id,
                preservarInput: true);

            if (resultado is not PageResult)
                return resultado;

            AdicionarErrosNosCampos(ex.Message);

            return Page();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<IActionResult> CarregarAsync(
        long id,
        bool preservarInput = false)
    {
        long usuarioId = User.ObterUsuarioId();

        try
        {
            Inspecao = await inspecaoService
                .BuscarPorIdAsync(id, usuarioId);

            var respostas = await inspecaoService
                .ListarRespostasAsync(id, usuarioId);

            if (!preservarInput)
            {
                Input = new SalvarRascunhoInput
                {
                    Respostas = respostas.Select(r =>
                        new RespostaChecklistInput
                        {
                            Id = r.Id,
                            Resposta = r.Resposta,
                            Observacao = r.Observacao
                        }).ToList()
                };
            }

            Itens = respostas;

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

    public List<RespostaInspecao> Itens { get; private set; } = new();

    public List<TipoResposta> OpcoesResposta { get; } =
        Enum.GetValues<TipoResposta>().ToList();


    private void AdicionarErrosNosCampos(string mensagem)
    {
        bool encontrouErro = false;
    
        foreach (var entrada in Itens.Select((valor, indice) =>
                     new { valor, indice }))
        {
            string nome = entrada.valor.ItemTitulo;
    
            string chaveResposta =
                $"Input.Respostas[{entrada.indice}].Resposta";
    
            string chaveObservacao =
                $"Input.Respostas[{entrada.indice}].Observacao";
    
            string erroResposta =
                $"O item '{nome}' não foi respondido.";
    
            string erroObservacao =
                $"O item '{nome}' exige observação.";
    
            if (mensagem.Contains(
                erroResposta,
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    chaveResposta,
                    "Selecione uma resposta para este item.");
    
                encontrouErro = true;
            }
    
            if (mensagem.Contains(
                erroObservacao,
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    chaveObservacao,
                    "Informe uma observação para este item pendente.");
    
                encontrouErro = true;
            }
        }
    
        if (!encontrouErro)
        {
            ModelState.AddModelError(
                string.Empty,
                mensagem);
        }
    }

}