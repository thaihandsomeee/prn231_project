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
    public class UpdateModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IHubContext<HubServer> hubContext;

        [BindProperty]
        public List<Models.Category>? category { get; set; }

        [BindProperty]
        public Models.Product? product { get; set; }

        public UpdateModel(PRN221_DBContext prn221DBContext,
        IHubContext<HubServer> hubContext)
        {
            this.prn221DBContext = prn221DBContext;
            this.hubContext = hubContext;
        }
        public async Task OnGet(int id)
        {
            ViewData["CategoryId"] = new SelectList(prn221DBContext.Categories, "CategoryId", "CategoryName");
            await LoadProduct(id);
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var productInDB = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == product.ProductId);
            if(productInDB is not null)
            {
                productInDB.ProductName = product.ProductName;
                productInDB.UnitPrice = product.UnitPrice;
                productInDB.QuantityPerUnit = product.QuantityPerUnit;
                productInDB.UnitsInStock = product.UnitsInStock;
                productInDB.CategoryId = product.CategoryId;
                productInDB.Discontinued = product.Discontinued;
                await prn221DBContext.SaveChangesAsync();
            }
            await hubContext.Clients.All.SendAsync("ReloadProduct"
                , await prn221DBContext.Products.ToListAsync());
            return RedirectToPage("/admin/product/index", new { txtSearch = product.ProductName });
        }

        private async Task LoadProduct(int id)
        {
            product = await prn221DBContext.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.ProductId == id);
        }
    }
}
