using Online_Store.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Online_Store.Services
{
    public class ProductService
    {
        private readonly OnlineStoreDbContext _db;

        public ProductService(OnlineStoreDbContext db)
        {
            _db = db;
        }

        public PagedResult<Product> SearchProducts(ProductFilter f)
        {
            
            var query = _db.Products.AsQueryable();

           
            if (!string.IsNullOrEmpty(f.SearchText))
                query = query.Where(p => p.Name.Contains(f.SearchText));
 

            if (f.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == f.CategoryId.Value);

            
            if (f.MinPrice.HasValue)
                query = query.Where(p => p.Price >= f.MinPrice.Value);

            
            if (f.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= f.MaxPrice.Value);

            
            if (f.InStockOnly == true)
                query = query.Where(p => p.Stock > 0);

             
            query = f.SortBy?.ToLower() switch
            {
                "price" => f.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "rating" => f.SortDescending ? query.OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating) ?? 0)
                                             : query.OrderBy(p => p.Reviews.Average(r => (double?)r.Rating) ?? 0),
                _ => f.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            
            var totalCount = query.Count();

            
            var items = query
                .Skip((f.PageNumber - 1) * f.PageSize)
                .Take(f.PageSize)
                .ToList();

            return new PagedResult<Product> { Items = items, TotalCount = totalCount };
        }
    }
}      
