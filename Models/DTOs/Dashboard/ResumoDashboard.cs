namespace PassoCerto.Web.Models.DTOs.Dashboard;

public class ResumoDashboard
{
    public long TotalInspecoes { get; set; }
    public long Concluidas { get; set; }
    public long Rascunhos { get; set; }

    public decimal? PercentualMedio { get; set; }

    public long TotalLojasAtivas { get; set; }
    public long LojasInspecionadas { get; set; }

    public long QuantidadeOk { get; set; }
    public long QuantidadePendentes { get; set; }
    public long QuantidadeNa { get; set; }
}