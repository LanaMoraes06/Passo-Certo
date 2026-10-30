namespace PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;

public class Inspecao
{
    public long Id { get; set; }

    public long LojaId { get; set; }
    public long RegiaoId { get; set; }

    public string GerenteNome { get; set; } = string.Empty;

    public long AutorId { get; set; }

    public DateOnly DataVisita { get; set; }

    public string? ObservacaoGeral { get; set; }

    public StatusInspecao Status { get; set; }

    public decimal? PercentualRealizacao { get; set; }

    public DateTime? ConcluidaEm { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public long CriadoPor { get; set; }
    public long AtualizadoPor { get; set; }
}