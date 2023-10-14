using Scriban;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazorPage.Models;
using Microsoft.EntityFrameworkCore;
using Aspose.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MyRazorPage.Pages.Account
{
    [Authorize(Roles = "2")]
    public class InvoiceModel : PageModel
    {
        private String file = @"D:\PRN221\WebDemo\MyRazorPage\MyRazorPage\Template\invoice.html";
        private String templateFile = @"D:\PRN221\WebDemo\MyRazorPage\MyRazorPage\Template\";
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;
        public InvoiceModel(PRN221_DBContext prn221DBContext, IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
        }
        public async Task <IActionResult> OnGet(int id)
        {
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fileStream))
                {
                    string templateContent = await sr.ReadToEndAsync();
                    var template = Scriban.Template.Parse(templateContent);
                    var templateData = GenerateData(id);
                    var pageContent = await template.RenderAsync(templateData);
                    System.IO.File.WriteAllText(templateFile + "template.html", pageContent);
                    HtmlLoadOptions options = new HtmlLoadOptions();
                    Document pdfDocument = new Document(templateFile + "template.html", options);
                    //export pdf
                    using (var stream = new MemoryStream())
                    {
                        pdfDocument.Save(stream);
                        return File(
                              stream.ToArray(),
                              "application/pdf",
                              "Order.pdf");

                    }
                }
            }
            return RedirectToPage("/order/allOrders");
        }

        public dynamic GenerateData(int orderId)
        {
            var company = new
            {
                Name = "Online shop",
                Phone = "(+84)241820311",
                Email = "company1@gmail.com"
            };

            var order = 
                prn221DBContext.Orders
                .Where(o => o.OrderId == orderId)
                .Select(x => new
                {
                Number = x.OrderId,
                Date = x.OrderDate,
                Customer = x.CustomerId
                }).First();

            var account =
                prn221DBContext.Accounts
                .Where(x => x.CustomerId == order.Customer)
                .Include(x => x.Customer)
                .Select(x => new
                {
                    Name = x.Customer.ContactName,
                    Address = x.Customer.Address,
                    Email = x.Email
                }).First();

            var orderDetail = 
                prn221DBContext.OrderDetails
                .Where(x => x.OrderId == orderId)
                .Include(x => x.Product)
                .Select(x => new
                {
                    Name = x.Product.ProductName,
                    Price = x.Product.UnitPrice,
                    Quantity = x.Quantity,
                    TotalPrice = x.Product.UnitPrice * x.Quantity
                })
                .ToList();

            var sum = orderDetail.Sum(x => x.TotalPrice);

            var data = new
            {
                Data = new
                {
                    Company = company,
                    Account = account,
                    Order = order,
                    OrderDetail = orderDetail,
                    SubTotal = sum.ToString(),
                    Total = sum.ToString()
                }

            };
            return data;
        }
    }
}
