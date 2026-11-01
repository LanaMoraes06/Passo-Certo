namespace PassoCerto.Web.Models.DTOs.Dashboard;

public class LojaNaoInspecionada
{
    public long LojaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Regiao { get; set; } = string.Empty;
    public DateTime? UltimaInspecao { get; set; }
    public string Nome { get; internal set; }
    public DateOnly? UltimaVisita { get; set; }}