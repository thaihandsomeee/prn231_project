using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DocumentFormat.OpenXml.InkML;

namespace MyRazorPage.Pages.Account
{
    public class SignOutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            if (HttpContext.Session.GetString("account") != null)
            {
                HttpContext.Session.Remove("account");
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToPage("/Index");
            }
            return Page();
        }
    }
}
