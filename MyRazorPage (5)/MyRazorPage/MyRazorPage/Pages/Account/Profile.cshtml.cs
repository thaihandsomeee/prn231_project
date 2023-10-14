using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.common;
using MyRazorPage.Models;
using System.Security.Principal;
using System.Text.Json;

namespace MyRazorPage.Pages.Account
{
    [Authorize(Roles = "2")]
    public class ProfileModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;

        public ProfileModel(PRN221_DBContext prn221DBContext) => this.prn221DBContext = prn221DBContext;

        [BindProperty]
        public Models.Account? account { get; set; }

        [BindProperty]
        public Models.Customer? customer { get; set; }

        public async Task<IActionResult> OnGet()
        {
            String? accountSession = HttpContext.Session.GetString("account");
            if (accountSession is not null)
            {
                account = JsonSerializer.Deserialize<Models.Account>(accountSession);
                if (account is not null && account.Role == CommonRole.CUSTOMER_ROLE)
                {
                    customer = await findByCustomerId(account.CustomerId);
                    return Page();
                }
            }
            return Redirect("signIn");
        }

        public async Task<Models.Customer?> findByCustomerId(String? customerId)
        {
            var customer = await prn221DBContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
            if (customer is not null)
            {
                return customer;
            }
            return null;
        }

    }
}
