using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using MyRazorPage.SignalR;

namespace MyRazorPage.Pages.Admin.Order
{
    [Authorize(Roles = "1")]
    public class CancelModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IHubContext<HubServer> hubContext;

        public CancelModel(PRN221_DBContext prn221DBContext,
        IHubContext<HubServer> hubContext)
        {
            this.prn221DBContext = prn221DBContext;
            this.hubContext = hubContext;
        }
        public async Task<IActionResult> OnGet(int? id)
        {
            if(id is null)
            {
                return RedirectToPage("/admin/order/index");
            }
            var order = await prn221DBContext.Orders.SingleOrDefaultAsync(x => x.OrderId == id);
            if(order is not null)
            {
                order.RequiredDate = null;
                await prn221DBContext.SaveChangesAsync();
            }
            await hubContext.Clients.All.SendAsync("ReloadOrder"
            , await prn221DBContext.Orders.ToListAsync());
            return RedirectToPage("/admin/order/index");
        }
    }
}
