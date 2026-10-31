using System.ComponentModel.DataAnnotations;

namespace PassoCerto.Web.Models.DTOs.Auth;

public class LoginInput
{
    [Required(ErrorMessage = "Informe seu login.")]
    [StringLength(100, ErrorMessage =
        "O login deve ter no máximo 100 caracteres.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua senha.")]
    public string Senha { get; set; } = string.Empty;
}