using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PassoCerto.Web.Pages.Admin;

[Authorize(Policy = "SomenteAdministrador")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}