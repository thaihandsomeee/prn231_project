using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.common;
using MyRazorPage.Models;
using System.Security.Principal;
using System.Text.Json;

namespace MyRazorPage.Pages.Admin
{
    [Authorize(Roles = "1")]
    public class DashboardModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        public DashboardModel(PRN221_DBContext dBContext)
        => this.prn221DBContext = dBContext;

        [BindProperty]
        public long totalOrder { get; set; }

        [BindProperty]
        public long totalOrderWeekly { get; set; }

        [BindProperty]
        public long totalCustomer { get; set; }

        [BindProperty]
        public long newCustomer { get; set; }

        [BindProperty]
        public long totalGuest { get; set; }
        public HashSet<int> years { get; set; }
        public long jan { get; set; }
        public long feb { get; set; }
        public long mar { get; set; }
        public long apr { get; set; }
        public long may { get; set; }
        public long jun { get; set; }
        public long jul { get; set; }
        public long aug { get; set; }
        public long sep { get; set; }
        public long oct { get; set; }
        public long nov { get; set; }
        public long dec { get; set; }

       
        public async Task OnGet(int? ddlYears)
        {

            await LoadTotalOrders();
            await LoadTotalOrdersWeekly();
            await LoadTotalCustomer();
            await LoadTotalGuest();
            await LoadNewCustomer();
            await LoadMonthlySale(ddlYears);
            LoadYear();
        }

        private async Task LoadTotalOrders()
        {
            totalOrder = (long)await prn221DBContext.Orders.SelectMany(o => o.OrderDetails).SumAsync(od => od.Quantity * od.UnitPrice);
        }

        private async Task LoadTotalOrdersWeekly()
        {
            DateTime startDate = DateTime.Now
                .AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek)
                .AddSeconds(-DateTime.Now.Second)
                .AddMinutes(-DateTime.Now.Minute)
                .AddHours(-DateTime.Now.Hour);

            totalOrderWeekly = (long)await prn221DBContext.Orders
                .Where(o => o.OrderDate > startDate && o.OrderDate < DateTime.Now)
                .SelectMany(o => o.OrderDetails)
                .SumAsync(od => od.Quantity * od.UnitPrice);
        }
        private async Task LoadTotalCustomer()
        {
            totalCustomer = await prn221DBContext.Accounts.Where(x => x.Role == CommonRole.CUSTOMER_ROLE).CountAsync();
        }

        private async Task LoadNewCustomer()
        {
            int count = 0;
            var newCusAndGuest = await prn221DBContext.Customers.Where(x => x.CreateDate >= DateTime.Now.AddDays(-30)).ToListAsync();
            foreach (var c in newCusAndGuest)
            {
                var customer = await prn221DBContext.Accounts.SingleOrDefaultAsync(x => x.CustomerId == c.CustomerId);
                if (customer is not null) count++;
            }
            newCustomer = count;
        }

        private async Task LoadTotalGuest()
        {
            totalGuest = (await prn221DBContext.Customers.CountAsync() - await prn221DBContext.Accounts
                .CountAsync());
        }

        private void LoadYear()
        {
            years = prn221DBContext.Orders.Select(x => x.OrderDate.Value.Year).ToHashSet();
        }

        private async Task LoadMonthlySale(int? year)
        {
            if (year is null) { year = DateTime.Now.Year; }
            var orders = await prn221DBContext.Orders
                .Where(x => x.OrderDate.Value.Year == year)
                .ToListAsync();
            foreach (var order in orders)
            {
                if (order.OrderDate.Value.Month == 1)
                {
                    jan += 1;
                }
                if (order.OrderDate.Value.Month == 2)
                {
                    feb += 1;
                }
                if (order.OrderDate.Value.Month == 3)
                {
                    mar += 1;
                }
                if (order.OrderDate.Value.Month == 4)
                {
                    apr += 1;
                }
                if (order.OrderDate.Value.Month == 5)
                {
                    may += 1;
                }
                if (order.OrderDate.Value.Month == 6)
                {
                    jun += 1;
                }
                if (order.OrderDate.Value.Month == 7)
                {
                    jul += 1;
                }
                if (order.OrderDate.Value.Month == 8)
                {
                    aug += 1;
                }
                if (order.OrderDate.Value.Month == 9)
                {
                    sep += 1;
                }
                if (order.OrderDate.Value.Month == 10)
                {
                    oct += 1;
                }
                if (order.OrderDate.Value.Month == 11)
                {
                    nov += 1;
                }
                if (order.OrderDate.Value.Month == 12)
                {
                    dec += 1;
                }
            }
            ViewData["year"] = year;
        }
    }
}
