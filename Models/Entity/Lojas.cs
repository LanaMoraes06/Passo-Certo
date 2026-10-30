namespace PassoCerto.Web.Models.Entity;

public class Loja
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string GerenteNome { get; set; } = string.Empty;
    public long RegiaoId { get; set; }
    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}