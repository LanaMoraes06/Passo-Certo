using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PassoCerto.Web.Models.DTOs.Auth;
using PassoCerto.Web.Services;

namespace PassoCerto.Web.Pages.Auth;

[AllowAnonymous]
public class LoginModel(AuthService authService) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var usuario = await authService.AutenticarAsync(Input);

        if (usuario is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Usuário ou senha inválidos.");

            return Page();
        }

        // Claims = informações da identidade autenticada.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Login),
            new(ClaimTypes.Role, usuario.Perfil.ToString().ToUpperInvariant())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        // Solicita ao ASP.NET Core a criação do cookie.
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false
            });

        return RedirectToPage("/Index");
    }
}
