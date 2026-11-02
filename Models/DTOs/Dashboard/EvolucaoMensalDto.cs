namespace PassoCerto.Web.Models.DTOs.Dashboard;

public class EvolucaoMensalDto
{
    public DateOnly Mes { get; set; }

    public decimal? PercentualMedio { get; set; }

    public long TotalInspecoes { get; set; }
}