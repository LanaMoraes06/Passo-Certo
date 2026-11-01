namespace PassoCerto.Web.Models.DTOs.Common;

public class ResultadoPaginado<T>
{
    public List<T> Itens { get; set; } = [];

    public int Pagina { get; set; }

    public int TamanhoPagina { get; set; }

    public long TotalRegistros { get; set; }
}