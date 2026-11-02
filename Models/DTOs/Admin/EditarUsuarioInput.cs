using System.ComponentModel.DataAnnotations;
using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Admin;

public class EditarUsuarioInput
{
    [Required(ErrorMessage = "Informe o login.")]
    [StringLength(100)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione um perfil.")]
    public PerfilUsuario? Perfil { get; set; }
}