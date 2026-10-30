namespace PassoCerto.Web.Models.Entity;

public class ItemChecklist
{
    public long Id { get; set; }
    public long CategoriaId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}