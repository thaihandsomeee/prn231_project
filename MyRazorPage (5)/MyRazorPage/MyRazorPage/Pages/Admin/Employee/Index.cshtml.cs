using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.common;
using MyRazorPage.Models;
using System.Text.Json;

namespace MyRazorPage.Pages.Admin.Employee
{
    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;

        [BindProperty]
        public Pager<Models.Account>? accounts { get; set; }

        [BindProperty]
        public int currentPage { get; set; }

        public IndexModel(PRN221_DBContext prn221DBContext,
        IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int pg, string? txtSearch)
        {
            String? accountSession = HttpContext.Session.GetString("account");
            if (accountSession is not null)
            {
                var account = JsonSerializer.Deserialize<Models.Account>(accountSession);
                if (account is not null && account.Role == CommonRole.EMPLOYEE_ROLE)
                {
                    if (pg < 1) pg = 1;
                    await getAllEmployees(pg, account.Email, txtSearch);
                    return Page();
                }
            }
            return Redirect("/account/signIn");
        }

        public async Task<IActionResult> OnGetActive(int id)
        {
            var account = await prn221DBContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == id);
            if (account is not null)
            {
              if(account.Status == true) account.Status = false;
              else account.Status = true;
              await  prn221DBContext.SaveChangesAsync();
            }
            return RedirectToPage("/admin/employee/index");
        }

            public async Task getAllEmployees(int? pageIndex, string? email, string? txtSearch)
        {
            IQueryable<Models.Account> accountsIQ = from account in prn221DBContext.Accounts
                                                    select account;
            if (txtSearch is not null)
            {
                accountsIQ = accountsIQ.Where(x => x.Email.Contains(txtSearch.Trim()));
                ViewData["txtSearch"] = txtSearch;
            }
            var pageSize = configuration.GetValue("TableSize", 8);
            accounts = await Pager<Models.Account>.CreateAsync(accountsIQ.Include(x => x.Employee).Where(x => x.Role == CommonRole.EMPLOYEE_ROLE && !x.Email.Equals(email))
                       .AsNoTracking(), pageIndex ?? 1, pageSize);
            if (accounts is not null)
            {
                currentPage = (int)Math.Ceiling((decimal)accounts.Count / (decimal)pageSize);
            }
        }
    }
}
