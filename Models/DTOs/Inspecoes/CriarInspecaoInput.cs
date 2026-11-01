using System.ComponentModel.DataAnnotations;

namespace PassoCerto.Web.Models.DTOs.Inspecoes;

public class CriarInspecaoInput
{
    [Required(ErrorMessage = "Selecione uma loja.")]
    public long? LojaId { get; set; }

    [Required(ErrorMessage = "Informe a data da visita.")]
    [DataType(DataType.Date)]
    public DateOnly? DataVisita { get; set; }

    [StringLength(1000)]
    public string? ObservacaoGeral { get; set; }
}