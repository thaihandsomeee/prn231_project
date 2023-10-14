using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.common;
using MyRazorPage.Common;
using MyRazorPage.Models;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace MyRazorPage.Pages.Account
{
    public class SignInModel : PageModel
    {

        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;
        public SignInModel(PRN221_DBContext prn221DBContext, IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
        }

        [BindProperty]
        public Models.Account? account { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                account = await findByEmailAndPassword(account.Email, account.Password);

                if (account is not null)
                {
                    var claims = new List<Claim>
                    {
                       new Claim(ClaimTypes.Email, account.Email),
                       new Claim(ClaimTypes.Role, account.Role.ToString()),
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme
                        , new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))
                        , new AuthenticationProperties
                        {
                            IsPersistent = true,
                            AllowRefresh = true,
                        });
                    HttpContext.Session.SetString("account", JsonSerializer.Serialize(account, new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles }));
                    if (account.Role == CommonRole.CUSTOMER_ROLE) return RedirectToPage("/index");
                    else if (account.Role == CommonRole.EMPLOYEE_ROLE) return RedirectToPage("/admin/dashboard");
                    else return Page();
                }
                else
                {
                    ViewData["message"] = "This account is not valid";
                    return Page();
                }
            }
            return Page();
        }

        public async Task<Models.Account?> findByEmailAndPassword(String? email, String? password)
        {
            var accountInDB = await prn221DBContext.Accounts
                .FirstOrDefaultAsync(x => x.Email == email);
            if (accountInDB is not null && accountInDB.Status == true)
            {
                if (HashPassword.DecryptString(configuration.GetValue<string>("SecretKey"), accountInDB.Password).Equals(password))
                {
                    return accountInDB;
                }
            }
            return null;
        }

    }
}
