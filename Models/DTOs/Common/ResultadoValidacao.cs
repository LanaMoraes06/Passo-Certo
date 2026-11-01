namespace PassoCerto.Web.Models.DTOs.Common;

public class ResultadoValidacao
{
    public List<string> Erros { get; set; } = new();

    public bool Valido => Erros.Count == 0;
}