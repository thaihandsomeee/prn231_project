using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MyRazorPage.Models
{
    public class Pager<T> : List<T> 
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }

        public Pager()
        {

        }

        public Pager(List<T> items, int totalItems, int pageIndex, int pageSize)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int startPage = pageIndex - 5;
            int endPage = pageIndex + 4;

            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }
            TotalItems = totalItems;
            CurrentPage = pageIndex;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
            this.AddRange(items);
        }

        public static async Task<Pager<T>> CreateAsync
            (IQueryable<T> source ,int pageIndex, int pageSize)
        {       
            var count = await source.CountAsync();
            if (count <= 0) return null;
            if (pageIndex > (int)Math.Ceiling((decimal)count / (decimal)pageSize)) pageIndex = (int)Math.Ceiling((decimal)count / (decimal)pageSize);
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new Pager<T>(items, count, pageIndex, pageSize);
        }

    }
}
