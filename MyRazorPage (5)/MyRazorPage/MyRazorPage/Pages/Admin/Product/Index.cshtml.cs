using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyRazorPage.Models;
using MyRazorPage.SignalR;
using OfficeOpenXml;
using System.Diagnostics;
using System.Drawing.Printing;

namespace MyRazorPage.Pages.Product
{
    [Authorize(Roles = "1")]
    public class ListModel : PageModel
    {
        private readonly String error = "Can't import Excel! Error in column ";
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;
        private readonly IHubContext<HubServer> hubContext;

        [BindProperty]
        public Pager<Models.Product>? products { get; set; }

        [BindProperty]
        public List<Models.Category>? categories { get; set; }

        [BindProperty]
        public int currentPage { get; set; }

        public String excelError { get; set; }

        public ListModel(PRN221_DBContext prn221DBContext,
            IConfiguration configuration, IHubContext<HubServer> hubContext)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
            this.hubContext = hubContext;
        }

        public async Task OnGet(int ddlCategory, int pg, String? txtSearch)
        {
            await LoadCategory();
            if (pg < 1) pg = 1;
            await LoadProduct(ddlCategory, pg, txtSearch);
        }

        //Todo:Export list product

        [Obsolete]
        public async Task<IActionResult> OnPost(IFormFile? file)
        {
            var listProductsFromExcel = new List<Models.Product>();

            if (file is not null)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        for (int i = worksheet.Dimension.Start.Row + 1;
                            i <= worksheet.Dimension.End.Row; i++)
                        {
                            try
                            {
                                int j = 0;
                                var productName = getProductName(worksheet.Cells[i, ++j]);
                                if (productName is null) return Page();
                                var unitPrice = getUnitPrice(worksheet.Cells[i, ++j]);
                                if (unitPrice is null) return Page();
                                var quantityPerUnit = getQuanityPerUnit(worksheet.Cells[i, ++j]);
                                if (quantityPerUnit is null) return Page();
                                var unitInStock = getUnitInStock(worksheet.Cells[i, ++j]);
                                if (unitInStock is null) return Page();
                                var categoryId = getCategory(worksheet.Cells[i, ++j]);
                                if (categoryId is null) return Page();
                                var discontinued = getDiscontinued(worksheet.Cells[i, ++j]);
                                if (discontinued is null) return Page();

                                listProductsFromExcel.Add(new Models.Product
                                {
                                    ProductName = productName,
                                    UnitPrice = unitPrice,
                                    QuantityPerUnit = quantityPerUnit,
                                    UnitsInStock = unitInStock,
                                    CategoryId = categoryId,
                                    Discontinued =  discontinued ?? false
                                });
                            }
                            catch (Exception ex)
                            {
                                return RedirectToPage("/admin/product/index");
                            }
                        }
                        await saveData(listProductsFromExcel);
                        await hubContext.Clients.All.SendAsync("ReloadData"
                        , await prn221DBContext.Products.ToListAsync());
                        return RedirectToPage("/admin/product/index");
                    }
                }
            }
            return Page();
        }

        private async Task saveData(List<Models.Product> listProductsFromExcel)
        {
            await prn221DBContext.Products.AddRangeAsync(listProductsFromExcel);
            await prn221DBContext.SaveChangesAsync();
        }
        private async Task LoadProduct(int ddlCategory, int? pageIndex, String? txtSearch)
        {
            IQueryable<Models.Product> productsIQ = from product in prn221DBContext.Products
                                                    select product;
            if (ddlCategory != 0)
            {
                productsIQ = productsIQ.Where(x => x.CategoryId == ddlCategory);
            }
            if (txtSearch is not null)
            {
                productsIQ = productsIQ.Where(x => x.ProductName.Contains(txtSearch.Trim()));
            }

            var pageSize = configuration.GetValue("TableSize", 8);
            products = await Pager<Models.Product>.CreateAsync(productsIQ.Include(x => x.Category).
                OrderByDescending(x => x.ProductId).AsNoTracking(), pageIndex ?? 1, pageSize);
            if (products is not null)
            {
                currentPage = (int)Math.Ceiling((decimal)products.Count / (decimal)pageSize);
            }
            ViewData["Category"] = ddlCategory;
            ViewData["Search"] = txtSearch;
        }
        private async Task LoadCategory()
        {
            categories = await prn221DBContext.Categories.ToListAsync();
        }
        private string? getProductName(ExcelRange excelRange)
        {
            var cellProductName = excelRange.Value;
            if (cellProductName is not null)
            {
                return cellProductName.ToString();
            }
            else
            {
                excelError = error + excelRange.Start.Address;
            }
            return null;
        }
        private decimal? getUnitPrice(ExcelRange excelRange)
        {
            var cellUnitPrice = excelRange.Value;
            if (cellUnitPrice is not null)
            {
                try
                {
                    return Convert.ToDecimal(cellUnitPrice);
                }
                catch (FormatException e)
                {
                    excelError = error + excelRange.Start.Address + "/ " + e.Message;
                }
            }
            else
            {
                excelError = error + excelRange.Start.Address;
            }
            return null;
        }
        private string? getQuanityPerUnit(ExcelRange excelRange)
        {
            var cellQuantityPerUnit = excelRange.Value;
            if (cellQuantityPerUnit is not null)
            {
                return cellQuantityPerUnit.ToString();
            }
            else
            {
                excelError = error + excelRange.Start.Address;
            }
            return null;
        }
        private short? getUnitInStock(ExcelRange excelRange)
        {
            var cellUnitInStock = excelRange.Value;
            if (cellUnitInStock is not null)
            {
                try
                {
                    return (short?)Convert.ToUInt32(cellUnitInStock);
                }
                catch (FormatException e)
                {
                    excelError = error + excelRange.Start.Address + "/ " + e.Message;
                }
            }
            else
            {
                excelError = error + excelRange.Start.Address;
            }
            return null;
        }
        private int? getCategory(ExcelRange excelRange)
        {
            var cellCategory = excelRange.Value;
            if (cellCategory is not null)
            {
                try
                {
                    return Convert.ToInt32(cellCategory);
                }
                catch (FormatException e)
                {
                    excelError = error + excelRange.Start.Address + "/ " + e.Message;
                }
            }
            else
            {
                excelError = error + excelRange.Start.Address;
            }
            return null;
        }
        private bool? getDiscontinued(ExcelRange excelRange)
        {
            var cellDiscontinued = excelRange.Value;
            if (cellDiscontinued is not null)
            {
                try
                {
                    return Convert.ToBoolean(cellDiscontinued);
                }
                catch (FormatException e)
                {
                    excelError = error + excelRange.Start.Address + "/ " + e.Message;
                }
            }
            else
            {
                excelError = error + excelRange.Start.Address;
            }
            return null;
        }
    }
}
