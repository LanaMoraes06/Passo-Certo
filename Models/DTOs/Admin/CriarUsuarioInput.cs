using System.ComponentModel.DataAnnotations;
using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Admin;

public class CriarUsuarioInput
{
    [Required(ErrorMessage = "Informe o login.")]
    [StringLength(100)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [MinLength(8, ErrorMessage = "Use pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione um perfil.")]
    public PerfilUsuario? Perfil { get; set; }
}