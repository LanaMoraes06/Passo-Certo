using System.ComponentModel.DataAnnotations;

namespace PassoCerto.Web.Models.DTOs.Admin;

public class ItemChecklistInput
{
    [Range(1, long.MaxValue, ErrorMessage = "Selecione uma categoria.")]
    public long CategoriaId { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(255)]
    public string Titulo { get; set; } = "";

    [StringLength(1000)]
    public string Descricao { get; set; } = "";

    [Range(1, int.MaxValue,
        ErrorMessage = "A ordem deve ser maior que zero.")]
    public int Ordem { get; set; } = 1;
}
