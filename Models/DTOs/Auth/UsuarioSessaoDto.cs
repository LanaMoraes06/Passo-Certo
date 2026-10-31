using PassoCerto.Web.Models.Enum;

namespace PassoCerto.Web.Models.DTOs.Auth;

public class UsuarioSessaoDto
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
}