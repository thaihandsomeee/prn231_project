using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using System.Text.Json;

namespace MyRazorPage.Pages.Account
{
    [Authorize(Roles = "2")]
    public class EditModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;

        public EditModel(PRN221_DBContext prn221DBContext) => this.prn221DBContext = prn221DBContext;

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
                if (account is not null)
                {
                    customer = await findByCustomerId(account.CustomerId);
                    return Page();
                }
            }
            return Redirect("signIn");
        }

        public async Task<ActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                String? getAccount = HttpContext.Session.GetString("account");
                var acc = JsonSerializer.Deserialize<Models.Account>(getAccount);
                var cus = await findByCustomerId(acc.CustomerId);
                if(cus is not null)
                {
                    cus.CompanyName = customer.CompanyName;
                    cus.ContactName = customer.ContactName;
                    cus.ContactTitle = customer.ContactTitle;
                    cus.Address = customer.Address;
                    await prn221DBContext.SaveChangesAsync();
                }
                if(acc is not null)
                {
                    acc.Email = account.Email;
                    await prn221DBContext.SaveChangesAsync();
                    return RedirectToPage("/account/signout");
                }
            }
            return Page();
        }

        public async Task<Models.Customer?> findByCustomerId(String? customerId)
        {
            var customer = await prn221DBContext.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);
            if (customer is not null)
            {
                return customer;
            }
            return null;
        }
    }
}
