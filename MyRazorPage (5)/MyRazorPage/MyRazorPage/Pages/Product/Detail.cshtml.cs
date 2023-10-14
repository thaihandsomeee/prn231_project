using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;

namespace MyRazorPage.Pages.Product
{
    public class DetailModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        public DetailModel(PRN221_DBContext prn221DBContext)
            => this.prn221DBContext = prn221DBContext;

        [BindProperty]
        public Models.Product? product { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            if (id != 0)
            {
                product = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == id);
                if(product is not null)
                {
                    if (product.ProductName.Contains("/deleted"))
                    {
                        return RedirectToPage("/index");
                    }
                    return Page();
                }
            }
            return RedirectToPage("/index");
        }
    }
}
