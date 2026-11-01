using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class InspecaoDetalheDto
{
    public long Id { get; set; }

    public string LojaNome { get; set; } = string.Empty;
    public string RegiaoNome { get; set; } = string.Empty;
    public string GerenteNome { get; set; } = string.Empty;
    public string AutorLogin { get; set; } = string.Empty;

    public DateOnly DataVisita { get; set; }
    public StatusInspecao Status { get; set; }
    public decimal? PercentualRealizacao { get; set; }

    public string? ObservacaoGeral { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public DateTime? ConcluidaEm { get; set; }

    public long CriadoPor { get; set; }
    public long AtualizadoPor { get; set; }

    public List<RespostaChecklistDto> Respostas { get; set; } = new();
}