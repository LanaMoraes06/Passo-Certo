namespace PassoCerto.Web.Models.DTOs.Dashboard;

public class ItemPendenteResumo
{
    public long ItemId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public long QuantidadePendencias { get; set; }

    public long LojasAfetadas { get; set; }
}