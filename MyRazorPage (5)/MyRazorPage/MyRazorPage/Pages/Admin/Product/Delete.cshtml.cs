using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using MyRazorPage.SignalR;

namespace MyRazorPage.Pages.Product
{
    [Authorize(Roles = "1")]
    public class DeleteModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IHubContext<HubServer> hubContext;

        public DeleteModel(PRN221_DBContext prn221DBContext,
        IHubContext<HubServer> hubContext)
        {
            this.prn221DBContext = prn221DBContext;
            this.hubContext = hubContext;
        }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            //Todo: delete product (check order detail before)
            var productOnOrdered = await prn221DBContext.OrderDetails.Where(x => x.ProductId == id).ToListAsync();
            if(productOnOrdered.Count > 0)
            {
                return RedirectToPage("/admin/product/index");
            }
            else
            {
                var product = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == id);
                if (product is not null)
                {
                    prn221DBContext.Products.Remove(product);
                    await prn221DBContext.SaveChangesAsync();

                }
            }
            await hubContext.Clients.All.SendAsync("ReloadProduct"
                , await prn221DBContext.Products.ToListAsync());
            return RedirectToPage("/admin/product/index");
        }
    }
}
