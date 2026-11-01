namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class EditarInspecaoInput
{
    public DateOnly DataVisita { get; set; }
    public string? ObservacaoGeral { get; set; }
}