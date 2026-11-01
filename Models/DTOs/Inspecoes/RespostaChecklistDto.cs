using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class RespostaChecklistDto
{
    public long Id { get; set; }

    public string CategoriaNome { get; set; } = string.Empty;
    public int CategoriaOrdem { get; set; }

    public string ItemTitulo { get; set; } = string.Empty;
    public string ItemDescricao { get; set; } = string.Empty;
    public int ItemOrdem { get; set; }

    public TipoResposta? Resposta { get; set; }
    public string? Observacao { get; set; }
}