using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using OfficeOpenXml.Style;
using System.Globalization;
using static OfficeOpenXml.ExcelErrorValue;

namespace MyRazorPage.Pages.Admin.Order
{
    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly String error = "Error: Start date after End date";
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;

        [BindProperty]
        public Pager<Models.Order>? orders { get; set; }

        [BindProperty]
        public int currentPage { get; set; }


        public IndexModel(PRN221_DBContext prn221DBContext,
            IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
        }

        public async Task<IActionResult> OnGet(DateTime? txtStartOrderDate, DateTime? txtEndOrderDate, int pg)
        {
            if (pg < 1) pg = 1;
            if (txtStartOrderDate > txtEndOrderDate)
            {
                TempData["error2"] = error;
                return RedirectToPage("/admin/order/index");
            }
            Console.Write(txtEndOrderDate);
            await getAllOrders(txtStartOrderDate, txtEndOrderDate, pg);
            return Page();
        }

        public async Task<IActionResult> OnGetExport(DateTime? txtStartOrderDate, DateTime? txtEndOrderDate)
        {
             var listOrders = await prn221DBContext.Orders.Include(x => x.Customer).Include(x => x.Employee).ToListAsync();
            if (txtStartOrderDate != null) listOrders = listOrders.Where(o => DateTime.Compare((DateTime)o.OrderDate, (DateTime)txtStartOrderDate) > 0).ToList();
            if (txtEndOrderDate != null) listOrders = listOrders.Where(o => DateTime.Compare((DateTime)o.OrderDate, (DateTime)txtEndOrderDate) < 0).ToList();

             var employees = await prn221DBContext.Employees.ToListAsync();
             var customers = await prn221DBContext.Customers.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Orders");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Order id";
                worksheet.Cell(currentRow, 2).Value = "Customer id";
                worksheet.Cell(currentRow, 3).Value = "Customer name";
                worksheet.Cell(currentRow, 4).Value = "Employee id";
                worksheet.Cell(currentRow, 5).Value = "Employee name";
                worksheet.Cell(currentRow, 6).Value = "Order date";
                worksheet.Cell(currentRow, 7).Value = "Required date";
                worksheet.Cell(currentRow, 8).Value = "Shipped date";
                worksheet.Cell(currentRow, 9).Value = "Freight";
                worksheet.Cell(currentRow, 10).Value = "Ship name";
                worksheet.Cell(currentRow, 11).Value = "Ship address";
                worksheet.Cell(currentRow, 12).Value = "Ship city";
                worksheet.Cell(currentRow, 13).Value = "Ship region";
                worksheet.Cell(currentRow, 14).Value = "Ship postal code";
                worksheet.Cell(currentRow, 15).Value = "Ship country";
                foreach (var order in listOrders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.OrderId;
                    worksheet.Cell(currentRow, 2).Value = order.CustomerId;

                    if (order.CustomerId is not null)
                    {
                        worksheet.Cell(currentRow, 3).Value = order.Customer.ContactName;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 3).Value = "";
                    }

                    worksheet.Cell(currentRow, 4).Value = order.EmployeeId;

                    if (order.EmployeeId is not null)
                    {
                        worksheet.Cell(currentRow, 5).Value = order.Employee.FirstName + " " + order.Employee.LastName;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "";
                    }

                    worksheet.Cell(currentRow, 6).Value = order.OrderDate;
                    worksheet.Cell(currentRow, 7).Value = order.RequiredDate;
                    worksheet.Cell(currentRow, 8).Value = order.ShippedDate;
                    worksheet.Cell(currentRow, 9).Value = order.Freight;
                    worksheet.Cell(currentRow, 10).Value = order.ShipName;
                    worksheet.Cell(currentRow, 11).Value = order.ShipAddress;
                    worksheet.Cell(currentRow, 12).Value = order.ShipCity;
                    worksheet.Cell(currentRow, 13).Value = order.ShipRegion;
                    worksheet.Cell(currentRow, 14).Value = order.ShipPostalCode;
                    worksheet.Cell(currentRow, 15).Value = order.ShipCountry;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "orders.xlsx");
                }
            }
            return RedirectToPage("/admin/order/index");
        }

        public async Task OnGetClear()
        {
            await getAllOrders(null, null, 1);
        }

        public async Task getAllOrders(DateTime? startDate, DateTime? endDate,int? pageIndex)
        {
            IQueryable<Models.Order> ordersIQ = from order in prn221DBContext.Orders     
                                                    select order;

            if(startDate is not null && endDate is not null)
            {
                ordersIQ = ordersIQ.Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate);
                ViewData["start"] = String.Format("{0:yyyy-MM-dd}", startDate);
                ViewData["end"] = String.Format("{0:yyyy-MM-dd}", endDate);
            }

            var pageSize = configuration.GetValue("TableSize", 8);
            orders = await Pager<Models.Order>.CreateAsync(ordersIQ.Include(x => x.Customer).Include(x => x.Employee).
                OrderByDescending(x => x.OrderDate).AsNoTracking(), pageIndex ?? 1, pageSize);
            if(orders is not null)
            {
                currentPage = (int)Math.Ceiling((decimal)orders.Count / (decimal)pageSize);
            }
        }
    }
}
