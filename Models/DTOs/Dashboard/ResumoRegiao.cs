namespace PassoCerto.Web.Models.DTOs.Dashboard;

public class ResumoRegiao
{
    public long RegiaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public long TotalLojas { get; set; }
    public long LojasInspecionadas { get; set; }
    public long TotalInspecoes { get; set; }
    public decimal? PercentualMedio { get; set; }
    public long Pendencias { get; set; }
}
