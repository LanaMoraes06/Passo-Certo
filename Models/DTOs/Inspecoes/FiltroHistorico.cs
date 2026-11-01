using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class FiltroHistorico
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }

    public long? RegiaoId { get; set; }
    public long? LojaId { get; set; }

    public StatusInspecao? Status { get; set; }

    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
}