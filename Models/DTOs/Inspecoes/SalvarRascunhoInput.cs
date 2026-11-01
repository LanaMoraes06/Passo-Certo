namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class SalvarRascunhoInput
{
    public List<RespostaChecklistInput> Respostas { get; set; } = new();
}