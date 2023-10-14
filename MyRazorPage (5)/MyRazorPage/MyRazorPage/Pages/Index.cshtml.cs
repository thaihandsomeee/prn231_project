using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPage.Models;

namespace MyRazorPage.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PRN221_DBContext prn221DBContext;
        private readonly IConfiguration configuration;

        [BindProperty]
        public List<Models.Category> categories { get; set; }
        [BindProperty]
        public List<Models.Product> bestSaleProducts { get; set; }

        [BindProperty]
        public Pager<Models.Product> products { get; set; }

        [BindProperty]
        public int currentPage { get; set; }

        public IndexModel(PRN221_DBContext prn221DBContext,
          IConfiguration configuration)
        {
            this.prn221DBContext = prn221DBContext;
            this.configuration = configuration;
        }

        public async Task OnGet(int? id, String? sort, int pg)
        {
            if (sort is null) sort = "name-asc";
            if (pg < 1) pg = 1;
            await loadCategories(id ?? 1, sort, pg);
            await loadProducts(id ?? 1, sort, pg);
            loadBestSaleProducts();
        }

        private async Task loadProducts(int id, String sortType, int? pageIndex)
        {
            IQueryable<Models.Product> productsIQ = from product in prn221DBContext.Products
                                                    where product.CategoryId == id
                                                    select product;

            //sort
            switch (sortType)
            {
                case "name-asc":
                    productsIQ = productsIQ.OrderBy(x => x.ProductName);
                    ViewData["sortName"] = "Name ascending";
                    break;
                case "name-desc":
                    productsIQ = productsIQ.OrderByDescending(x => x.ProductName);
                    ViewData["sortName"] = "Name descending";
                    break;
                case "price-asc":
                    productsIQ = productsIQ.OrderBy(x => x.UnitPrice);
                    ViewData["sortName"] = "Price ascending";
                    break;
                case "price-desc":
                    productsIQ = productsIQ.OrderByDescending(x => x.UnitPrice);
                    ViewData["sortName"] = "Price descending";
                    break;
                default:
                    productsIQ = productsIQ.OrderBy(x => x.ProductName);
                    ViewData["sortName"] = "Name ascending";
                    break;
            }
            //paging
            //remove deleted
            productsIQ = productsIQ.Where(x => !x.ProductName.Contains("/deleted"));
            var pageSize = configuration.GetValue("PageSize", 12);
            products = await Pager<Models.Product>.CreateAsync(productsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            currentPage = (int)Math.Ceiling((decimal)products.Count / (decimal)pageSize);
        }

        private async Task loadCategories(int id, String sortType, int pageIndex)
        {
            categories = prn221DBContext.Categories.ToList();
            var category = await prn221DBContext.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
            _ = category is null ? "" : ViewData["categoryType"] = category.CategoryName;
            ViewData["categoryId"] = id;
            ViewData["sortType"] = sortType;
        }

        private void loadBestSaleProducts()
        {
            var listOrderDetails = prn221DBContext.OrderDetails
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();
            var listMostOrderProducts = listOrderDetails
                .Select(id =>
                {
                    int count = prn221DBContext.OrderDetails
                                .Where(x => x.ProductId == id)
                                 .Count();
                    return new
                    {
                        ProductId = id,
                        Count = count
                    };
                })
                .OrderByDescending(x => x.Count)
                .ToList();
            var listBestSaleProdcutsId = listMostOrderProducts
                                       .Take(4)
                                       .Select(x => x.ProductId)
                                       .ToHashSet();
            bestSaleProducts = prn221DBContext.Products
                                .Where(x => listBestSaleProdcutsId
                                             .Contains(x.ProductId))
                                .ToList();
        }
    }
}
