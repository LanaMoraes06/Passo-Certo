namespace PassoCerto.Web.Models.Entity;

using PassoCerto.Web.Models.Enum;

public class RespostaInspecao
{
    public long Id { get; set; }

    public long InspecaoId { get; set; }
    public long ItemId { get; set; }

    public string CategoriaNome { get; set; } = string.Empty;
    public int CategoriaOrdem { get; set; }

    public string ItemTitulo { get; set; } = string.Empty;
    public string ItemDescricao { get; set; } = string.Empty;
    public int ItemOrdem { get; set; }

    // Resposta do supervisor
    public TipoResposta? Resposta { get; set; }
    public string? Observacao { get; set; }

    // Auditoria
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public long CriadoPor { get; set; }
    public long AtualizadoPor { get; set; }
}