namespace MobileShop.Web.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnPost() => RedirectToPage("/Index");
}
