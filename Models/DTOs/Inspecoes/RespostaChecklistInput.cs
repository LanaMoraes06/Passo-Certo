using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class RespostaChecklistInput
{
    public long Id { get; set; }

    public TipoResposta? Resposta { get; set; }

    public string? Observacao { get; set; }
}