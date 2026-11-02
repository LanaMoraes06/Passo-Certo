using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Extensions;
using PassoCerto.Web.Models.DTOs.Inspecoes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Services;
using System.Globalization;
using System.Text;

namespace PassoCerto.Web.Pages.Inspecoes;

[Authorize]
public class IndexModel(
    InspecaoService inspecaoService,
    LojaService lojaService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public FiltroHistorico Filtro { get; set; } = new();

    public List<InspecaoResumoDto> Inspecoes
        { get; private set; } = new();

    public List<Loja> Lojas
        { get; private set; } = new();

    public long TotalRegistros { get; private set; }

    public int TotalPaginas =>
        (int)Math.Ceiling(TotalRegistros / 10.0);

    public string? Erro { get; private set; }

    public async Task OnGetAsync()
    {
        long usuarioId = User.ObterUsuarioId();

        Lojas = await lojaService
            .ListarPermitidasAsync(usuarioId);

        if (!ModelState.IsValid)
        {
            Erro = "Verifique os filtros informados.";
            return;
        }

        try
        {
            var resultado = await inspecaoService
                .BuscarHistoricoAsync(Filtro, usuarioId);

            Inspecoes = resultado.Itens;
            TotalRegistros = resultado.Total;
        }
        catch (BusinessException ex)
        {
            Erro = ex.Message;
        }
    }

public async Task<IActionResult> OnGetExportarAsync()
{
    long usuarioId = User.ObterUsuarioId();

    if (!ModelState.IsValid)
        return BadRequest("Filtros inválidos.");

    var filtroExportacao = new FiltroHistorico
    {
        DataInicio = Filtro.DataInicio,
        DataFim = Filtro.DataFim,
        LojaId = Filtro.LojaId,
        RegiaoId = Filtro.RegiaoId,
        Status = Filtro.Status,
        Pagina = 1,
        TamanhoPagina = 10
    };

    var registros = new List<InspecaoResumoDto>();

    try
    {
        while (true)
        {
            var resultado = await inspecaoService
                .BuscarHistoricoAsync(
                    filtroExportacao, usuarioId);

            registros.AddRange(resultado.Itens);

            if (registros.Count >= resultado.Total ||
                resultado.Itens.Count == 0)
            {
                break;
            }

            filtroExportacao.Pagina++;
        }
    }
    catch (BusinessException ex)
    {
        ModelState.AddModelError(
            string.Empty, ex.Message);

        return BadRequest(ex.Message);
    }

    var csv = new StringBuilder();

    csv.AppendLine(
        "ID;Loja;Região;Data da visita;Status;Percentual");

    foreach (var item in registros)
    {
        string percentual = item.PercentualRealizacao.HasValue
            ? item.PercentualRealizacao.Value.ToString(
                "N1", CultureInfo.GetCultureInfo("pt-BR"))
            : "";

        csv.AppendLine(string.Join(";",
            item.Id.ToString(),
            EscaparCsv(item.LojaNome),
            EscaparCsv(item.RegiaoNome),
            item.DataVisita.ToString("dd/MM/yyyy"),
            item.Status.ToString(),
            percentual
        ));
    }

    byte[] conteudo = new UTF8Encoding(true)
        .GetPreamble()
        .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
        .ToArray();

    string nomeArquivo =
        $"historico-inspecoes-{DateTime.Now:yyyyMMdd-HHmmss}.csv";

    return File(
        conteudo,
        "text/csv; charset=utf-8",
        nomeArquivo);
}

private static string EscaparCsv(string? valor)
{
    valor ??= string.Empty;

    string inicio = valor.TrimStart();

    if (inicio.Length > 0 &&
        "=+-@".Contains(inicio[0]))
    {
        valor = "'" + valor;
    }

    return "\"" + valor.Replace("\"", "\"\"") + "\"";
}

}
