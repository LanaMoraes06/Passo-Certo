namespace PassoCerto.Web.Models.DTOs.Dashboard;

public class FiltroDashboard
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public long? RegiaoId { get; set; }
    public long? LojaId { get; set; }
}