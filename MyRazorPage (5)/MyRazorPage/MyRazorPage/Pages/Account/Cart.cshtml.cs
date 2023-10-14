using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;
using MyRazorPage.SignalR;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Security.Principal;
using System.Text.Json;
using System.Net.Mime;

namespace MyRazorPage.Pages.Account
{
    public class CartModel : PageModel
    {
        private String file = @"D:\PRN221\WebDemo\MyRazorPage\MyRazorPage\Template\mailInvoice.html";
        private String imageSrc = @"D:\PRN221\WebDemo\MyRazorPage\MyRazorPage\wwwroot\img\logo.png";
        private String templateFile = @"D:\PRN221\WebDemo\MyRazorPage\MyRazorPage\Template\";
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IHubContext<HubServer> hubContext;
        private readonly IConfiguration configuration;
        private readonly int lENGTH_CUSTOMER_ID = 5;
        private readonly Random _random = new();
        public CartModel(PRN221_DBContext prn221DBContext,
        IHubContext<HubServer> hubContext,
        IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.hubContext = hubContext;
            this.configuration = configuration;
        }

        [BindProperty]
        public List<Cart>? carts { get; set; }

        [BindProperty]
        public Models.Customer? customer { get; set; }

        public async Task<IActionResult> OnGet()
        {
            string? getCart = HttpContext.Session.GetString("cart");
            string? accountSession = HttpContext.Session.GetString("account");

            if (getCart is not null)
            {
                carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
                if (carts is not null)
                {
                    ViewData["TotalPrice"] = carts.Sum(x => x.Quantity * x.Product.UnitPrice);
                    ViewData["Quantity"] = carts.Sum(x => x.Quantity);
                }
                else
                {
                    return Redirect("/index");
                }
            }

            if (accountSession is not null)
            {
                var account = JsonSerializer.Deserialize<Models.Account>(accountSession);
                if (account is not null)
                {
                    customer = await findByCustomerId(account.CustomerId);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnGetBuyNow(int id)
        {

            bool isAddToCart = await AddToCart(id);
            if (isAddToCart)
            {
                return RedirectToPage("/account/cart");
            }
            return Redirect("/index");
        }

        public async Task<IActionResult> OnGetAddToCart(int id)
        {
            bool isAddToCart = await AddToCart(id);
            return Redirect("/product/detail?id=" + id);
        }

        public IActionResult OnGetMinus(int id)
        {
            string? getCart = HttpContext.Session.GetString("cart");
            if (getCart is not null)
            {
                carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
            }
            if (carts is not null)
            {
                int cartIndex = Exists(carts, id);
                if (cartIndex != -1)
                {
                    if (carts[cartIndex].Quantity > 1)
                    {
                        carts[cartIndex].Quantity--;
                    }
                }
                string saveCart = JsonSerializer.Serialize(carts);
                HttpContext.Session.SetString("cart", saveCart);
            }

            return RedirectToPage("/account/cart");
        }


        public IActionResult OnGetDelete(int id)
        {
            string? getCart = HttpContext.Session.GetString("cart");
            List<Cart>? carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
            int index = Exists(carts, id);
            carts.RemoveAt(index);
            string savesjoncart = JsonSerializer.Serialize(carts);
            HttpContext.Session.SetString("cart", savesjoncart);
            if (carts.Count > 0)
            {
                return RedirectToPage("/account/cart");
            }
            return Redirect("/index");
        }

        public async Task<IActionResult> OnPostOrder()
        {
            var session = HttpContext.Session.GetString("account");
            var acc = new Models.Account();
            var cus = new Customer();

            if (session is not null)
            {
                acc = JsonSerializer.Deserialize<MyRazorPage.Models.Account>(session);
                cus = prn221DBContext.Customers.SingleOrDefault(x => x.CustomerId == acc.CustomerId);
            }
            else
            {
                cus = new Customer
                {
                    CustomerId = generatedCustomerId(),
                    CompanyName = customer.CompanyName,
                    ContactName = customer.ContactName,
                    ContactTitle = customer.ContactTitle,
                    Address = customer.Address
                };
                await prn221DBContext.Customers.AddAsync(cus);
                await prn221DBContext.SaveChangesAsync();
            }


            Models.Order order = new Models.Order()
            {
                CustomerId = cus.CustomerId,
                OrderDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddDays(1),
            };

            await prn221DBContext.Orders.AddAsync(order);
            await prn221DBContext.SaveChangesAsync();


            string? jsoncart = HttpContext.Session.GetString("cart");
            List<Cart>? carts = JsonSerializer.Deserialize<List<Cart>>(jsoncart);
            {
                foreach (var product in carts)
                {
                    OrderDetail orderDetail = new();
                    orderDetail.OrderId = order.OrderId;
                    orderDetail.ProductId = product.Product.ProductId;
                    orderDetail.UnitPrice = (decimal)product.Product.UnitPrice;
                    orderDetail.Quantity = product.Quantity;
                    orderDetail.Discount = 0;
                    await prn221DBContext.OrderDetails.AddAsync(orderDetail);
                    await prn221DBContext.SaveChangesAsync();
                    var updateProduct = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == product.Product.ProductId);
                    if(updateProduct is not null)
                    {
                        updateProduct.UnitsInStock = (short?)(updateProduct.UnitsInStock - product.Quantity);
                        updateProduct.UnitsOnOrder = (short?)(updateProduct.UnitsOnOrder + product.Quantity);
                        await prn221DBContext.SaveChangesAsync();
                    }
                }
            };
            HttpContext.Session.Remove("cart");
            await hubContext.Clients.All.SendAsync("ReloadOrder"
            , await prn221DBContext.Orders.ToListAsync());
            await SendEmail(order.OrderId);
            return Redirect("/index");
        }

        public async Task SendEmail(int orderId)
        {
            await BindingData(orderId);
            string body = string.Empty;
            using (var fileStream = new FileStream(templateFile + "templateMail.html", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    body = await sr.ReadToEndAsync();
                }
                // Create a LinkedResource object for each embedded image
                ContentType imgType = new ContentType("image/png");
                LinkedResource inline = new LinkedResource(imageSrc, imgType);
                inline.ContentId = "companyLogo";
                AlternateView alterHtml = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alterHtml.LinkedResources.Add(inline);
                //send mail
                using (MailMessage mail = new MailMessage())
                {
                    SmtpClient SmtpServer = new SmtpClient();
                    mail.From = new MailAddress(configuration.GetValue<string>("Smtp:FromAddress"));
                    mail.To.Add("supersixevo54@gmail.com");
                    mail.Subject = "Order confirmation";
                    mail.AlternateViews.Add(alterHtml);
                    mail.IsBodyHtml = true;
                    SmtpServer.UseDefaultCredentials = false;
                    NetworkCredential NetworkCred = new NetworkCredential(
                        configuration.GetValue<string>("Smtp:UserName"),
                        configuration.GetValue<string>("Smtp:Password")
                        );
                    SmtpServer.Credentials = NetworkCred;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Port = configuration.GetValue<int>("Smtp:Port");
                    SmtpServer.Host = configuration.GetValue<string>("Smtp:Server");
                    SmtpServer.Send(mail);
                }
            }
        }
        private async Task BindingData(int orderId)
        {
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    string templateContent = await sr.ReadToEndAsync();
                    var template = Scriban.Template.Parse(templateContent);
                    var templateData = GenerateData(orderId);
                    var pageContent = await template.RenderAsync(templateData);
                    System.IO.File.WriteAllText(templateFile + "templateMail.html", pageContent);
                }
            }
        }

        private dynamic GenerateData(int orderId)
        {

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
                    Number = x.CustomerId,
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
            var items = orderDetail.Count;

            var data = new
            {
                Data = new
                {
                    Date = DateTime.Now.ToString("MM/dd/yyyy"),
                    Account = account,
                    Order = order,
                    OrderDetail = orderDetail,
                    SubTotal = sum.ToString(),
                    Total = sum.ToString(),
                    Items = items.ToString()
                }

            };
            return data;
        }

        private async Task<bool> AddToCart(int id)
        {
            var productOnOrder = await prn221DBContext.Products.SingleOrDefaultAsync(x => x.ProductId == id);
            string? getCart = HttpContext.Session.GetString("cart");
            if (getCart is not null)
            {
                carts = JsonSerializer.Deserialize<List<Cart>>(getCart);
            }
            if (carts is null)
            {
                carts = new List<Cart>();
                carts.Add(new Cart
                {
                    Product = productOnOrder,
                    Quantity = 1
                });
                string saveCart = JsonSerializer.Serialize(carts);
                HttpContext.Session.SetString("cart", saveCart);
                return true;
            }
            else
            {
                int cartIndex = Exists(carts, id);
                if (cartIndex == -1)
                {
                    carts.Add(new Cart
                    {
                        Product = productOnOrder,
                        Quantity = 1
                    });
                }
                else
                {
                    if(carts[cartIndex].Quantity < productOnOrder.UnitsInStock)
                    {
                        carts[cartIndex].Quantity++;
                    }
                }
                string saveCart = JsonSerializer.Serialize(carts);
                HttpContext.Session.SetString("cart", saveCart);
                return true;

            }
        }

        private int Exists(List<Cart> cart, int id)
        {
            for (var i = 0; i < cart.Count; i++)
            {
                if (cart[i].Product.ProductId == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public async Task<Models.Customer?> findByCustomerId(String? customerId)
        {
            var customer = await prn221DBContext.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);
            if (customer is not null)
            {
                return customer;
            }
            return null;
        }

        private string generatedCustomerId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, lENGTH_CUSTOMER_ID)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }

}
