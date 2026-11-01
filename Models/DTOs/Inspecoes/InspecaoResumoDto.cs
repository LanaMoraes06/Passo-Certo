using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class InspecaoResumoDto
{
    public long Id { get; set; }

    public string LojaNome { get; set; } = string.Empty;
    public string RegiaoNome { get; set; } = string.Empty;

    public DateOnly DataVisita { get; set; }

    public StatusInspecao Status { get; set; }

    public decimal? PercentualRealizacao { get; set; }
}