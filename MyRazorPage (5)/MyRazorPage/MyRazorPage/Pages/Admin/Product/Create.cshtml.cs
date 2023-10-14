using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using MyRazorPage.SignalR;

namespace MyRazorPage.Pages.Product
{
    [Authorize(Roles = "1")]
    public class CreateModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IHubContext<HubServer> hubContext;

        [BindProperty]
        public List<Models.Category>? category { get; set; }

        [BindProperty]
        public Models.Product product { get; set; }

        public CreateModel(PRN221_DBContext prn221DBContext,
        IHubContext<HubServer> hubContext)
        {
            this.prn221DBContext = prn221DBContext;
            this.hubContext = hubContext;
        }
        public async Task OnGet()
        {
            //await LoadCategories();
            ViewData["CategoryId"] = new SelectList(prn221DBContext.Categories, "CategoryId", "CategoryName");

        }

        public async Task<IActionResult> OnPost()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            await prn221DBContext.Products.AddAsync(product);
            await prn221DBContext.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReloadProduct"
                , await prn221DBContext.Products.ToListAsync());
            return RedirectToPage("/admin/product/index");
        }

        private async Task LoadCategories()
        {
            category = await prn221DBContext.Categories.ToListAsync();
        }
    }
}
