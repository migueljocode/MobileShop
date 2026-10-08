namespace MobileShop.Web.Pages.Account;

using Microsoft.AspNetCore.Authentication;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }
}
