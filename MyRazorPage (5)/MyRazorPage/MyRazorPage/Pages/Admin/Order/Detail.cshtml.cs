using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;

namespace MyRazorPage.Pages.Admin.Order
{
    [Authorize(Roles = "1")]
    public class DetailModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        public DetailModel(PRN221_DBContext prn221DBContext)
            => this.prn221DBContext = prn221DBContext;

        [BindProperty]
        public Models.Order? order { get; set; }

        [BindProperty]
        public List<Models.OrderDetail>? orderDetails { get; set; } 

        public async Task OnGet(int id)
        {
            await findByOrderId(id);
            await findListByOrderId(id);
        }

        public async Task findByOrderId(int id)
        {
            order = await prn221DBContext.Orders.SingleOrDefaultAsync(x => x.OrderId == id);
        }

        public async Task findListByOrderId(int id)
        {
            orderDetails = await prn221DBContext.OrderDetails.Include(x => x.Product).Where(x => x.OrderId == id).ToListAsync();
        }
    }
}
