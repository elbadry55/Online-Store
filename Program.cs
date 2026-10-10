using Microsoft.EntityFrameworkCore;
using Online_Store;
using Online_Store.Models;
using Online_Store.Services;
using System;
using System.Linq;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static void Main(string[] args)
    {
      
        var optionsBuilder = new DbContextOptionsBuilder<OnlineStoreDbContext>();
        optionsBuilder.UseSqlServer(connectionString: "Server=(localdb)\\mssqllocaldb;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True");

        using (var db = new OnlineStoreDbContext(optionsBuilder.Options))
        {
            // ==========================================
            // Level 1: Warm-up Answers
            // ==========================================

            // 1. For every delivered order, return OrderId, CustomerName, and Total, ordered by Total descending
            var q1 = db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Select(o => new
                {
                    OrderId = o.Id,
                    CustomerName = o.Customer.FullName,
                    Total = o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))
                })
                .OrderByDescending(x => x.Total)
                .ToList();


            // 2. Count how many orders exist per status, with the percentage of the whole
            var totalOrders = db.Orders.Count();
            var q2 = db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = totalOrders == 0 ? 0 : (double)g.Count() / totalOrders * 100
                })
                .ToList();


            // 3. Return products whose price is above the average price of their own category
            var q3 = db.Products
                .Where(p => p.Price > db.Products
                    .Where(p2 => p2.CategoryId == p.CategoryId)
                    .Average(p2 => p2.Price))
                .ToList();


            // 4. Return products that were never ordered
            var q4 = db.Products
                .Where(p => !p.OrderItems.Any())
                .ToList();


            // 5. Return customers who registered in the last 30 days and have at least one order
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var q5 = db.Customers
                .Where(c => c.RegisteredAt >= thirtyDaysAgo && c.Orders.Any())
                .ToList();


            // 6. Return customers who cancelled more than 2 orders, with the number of cancelled orders
            var q6 = db.Customers
                .Select(c => new
                {
                    Customer = c,
                    CancelledCount = c.Orders.Count(o => o.Status == OrderStatus.Cancelled)
                })
                .Where(x => x.CancelledCount > 2)
                .Select(x => new
                {
                    x.Customer,
                    CancelledOrdersCount = x.CancelledCount
                })
                .ToList();

            // ==========================================
            // Level 2: Grouping and aggregation
            // ==========================================

            // 7. Top 5 customers by total spending: name, order count, total spent, average order value
            var q7 = db.Customers
                .Select(c => new {
                    Name = c.FullName,
                    OrderCount = c.Orders.Count(o => o.Status == OrderStatus.Delivered),
                    TotalSpent = c.Orders
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .SelectMany(o => o.Items)
                        .Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount)),
                    AverageOrderValue = c.Orders
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .Any() ? c.Orders
                            .Where(o => o.Status == OrderStatus.Delivered)
                            .Average(o => o.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))) : 0
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToList();


            // 8. Monthly report for 2025: Year, Month, OrderCount, Revenue, ordered by month
            var q8 = db.Orders
                .Where(o => o.OrderDate.Year == 2025)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    OrderCount = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Delivered)
                                .SelectMany(o => o.Items)
                                .Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))
                })
                .OrderBy(x => x.Month)
                .ToList();


            // 9. Revenue per category, ordered by revenue descending
            var q9 = db.Categories
                .Select(c => new {
                    CategoryName = c.Name,
                    Revenue = c.Products
                        .SelectMany(p => p.OrderItems)
                        .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                        .Sum(oi => oi.Quantity * oi.UnitPrice * (1 - oi.Discount))
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();


            // 10. Count customers who never placed an order, grouped by city, ordered by count descending
            var q10 = db.Customers
                .Where(c => !c.Orders.Any())
                .GroupBy(c => c.City)
                .Select(g => new {
                    City = g.Key,
                    CustomerCount = g.Count()
                })
                .OrderByDescending(x => x.CustomerCount)
                .ToList();


            // 11. Products with at least 10 reviews and an average rating of 4.5 or more: name, category, review count, average rating
            var q11 = db.Products
                .Where(p => p.Reviews.Count >= 10 && p.Reviews.Average(r => (double)r.Rating) >= 4.5)
                .Select(p => new {
                    ProductName = p.Name,
                    CategoryName = p.Category.Name,
                    ReviewCount = p.Reviews.Count,
                    AverageRating = p.Reviews.Average(r => (double)r.Rating)
                })
                .ToList();


            // 12. For each category, return the top 2 most expensive products
            var q12 = db.Categories
                .Select(c => new {
                    CategoryName = c.Name,
                    TopProducts = c.Products
                        .OrderByDescending(p => p.Price)
                        .Take(2)
                        .Select(p => new { p.Name, p.Price })
                        .ToList()
                })
                .ToList();


            // 13. Products whose stock is lower than the total quantity in Pending orders. Show name, stock, and pending quantity
            var q13 = db.Products
                .Select(p => new {
                    Product = p,
                    PendingQty = p.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Pending)
                        .Sum(oi => (int?)oi.Quantity) ?? 0
                })
                .Where(x => x.Product.Stock < x.PendingQty)
                .Select(x => new {
                    Name = x.Product.Name,
                    Stock = x.Product.Stock,
                    PendingQuantity = x.PendingQty
                })
                .ToList();


            // 14. Customers whose total spending is above the average spending of all customers
            var customerSpendingQuery = db.Customers.Select(c => new {
                Customer = c,
                TotalSpent = c.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .SelectMany(o => o.Items)
                    .Sum(i => (decimal?)(i.Quantity * i.UnitPrice * (1 - i.Discount))) ?? 0
            }).ToList();

            var averageSpending = customerSpendingQuery.Any() ? customerSpendingQuery.Average(x => x.TotalSpent) : 0;

            var q14 = customerSpendingQuery
                .Where(x => x.TotalSpent > averageSpending)
                .Select(x => x.Customer)
                .ToList();

            // ==========================================
            // Level 3: Joins, subqueries, set logic
            // ==========================================

            // 15. For each category, return its best-selling product by quantity. Categories with no sales must still appear
            var q15 = db.Categories
                .Select(c => new {
                    CategoryName = c.Name,
                    BestSellingProduct = c.Products
                        .Select(p => new {
                            Product = p,
                            TotalQty = p.OrderItems
                                .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                                .Sum(oi => (int?)oi.Quantity) ?? 0
                        })
                        .OrderByDescending(x => x.TotalQty)
                        .Select(x => x.Product.Name)
                        .FirstOrDefault()
                })
                .ToList();


            // 16. Customers who bought every product in the category "Laptops"
            var laptopProductIds = db.Products
                .Where(p => p.Category.Name == "Laptops")
                .Select(p => p.Id)
                .ToList();

            var q16 = db.Customers
                .Where(c => laptopProductIds.All(laptopId =>
                    c.Orders
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .SelectMany(o => o.Items)
                        .Any(i => i.ProductId == laptopId)
                ))
                .ToList();


            // 17. Reviews written by a customer for a product they never bought
            var q17 = db.Reviews
                .Where(r => !db.Orders
                    .Any(o => o.CustomerId == r.CustomerId &&
                              o.Status == OrderStatus.Delivered &&
                              o.Items.Any(i => i.ProductId == r.ProductId)))
                .ToList();


            // 18. Return all customers with their order count, including customers with zero orders (left join with GroupJoin)
            var q18 = db.Customers
                .GroupJoin(
                    db.Orders.Where(o => o.Status == OrderStatus.Delivered),
                    c => c.Id,
                    o => o.CustomerId,
                    (c, orders) => new {
                        CustomerName = c.FullName,
                        OrderCount = orders.Count()
                    }
                )
                .ToList();


            // 19. The top 5 product pairs that appear together in the same order most often
            var q19 = db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.Items, (o, item1) => new { o.Id, Product1 = item1.ProductId })
                .Join(
                    db.Orders.Where(o => o.Status == OrderStatus.Delivered)
                        .SelectMany(o => o.Items, (o, item2) => new { o.Id, Product2 = item2.ProductId }),
                    x => x.Id,
                    y => y.Id,
                    (x, y) => new { x.Product1, y.Product2 }
                )
                .Where(p => p.Product1 < p.Product2)
                .GroupBy(p => new { p.Product1, p.Product2 })
                .Select(g => new {
                    Product1 = g.Key.Product1,
                    Product2 = g.Key.Product2,
                    PairCount = g.Count()
                })
                .OrderByDescending(x => x.PairCount)
                .Take(5)
                .ToList();


            // 20. For each customer, return their name and their most recent order with each item's product name and quantity, in one query
            var q20 = db.Customers
                .Select(c => new {
                    CustomerName = c.FullName,
                    MostRecentOrder = c.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .Select(o => new {
                            o.Id,
                            o.OrderDate,
                            Items = o.Items.Select(i => new {
                                ProductName = i.Product.Name,
                                i.Quantity
                            })
                        })
                        .FirstOrDefault()
                })
                .ToList();


            // 21. For each customer, return their favorite category (the category with the highest total quantity bought)
            var q21 = db.Customers
                .Select(c => new {
                    CustomerName = c.FullName,
                    FavoriteCategory = c.Orders
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .SelectMany(o => o.Items)
                        .GroupBy(i => i.Product.Category.Name)
                        .Select(g => new {
                            CategoryName = g.Key,
                            TotalQty = g.Sum(i => i.Quantity)
                        })
                        .OrderByDescending(x => x.TotalQty)
                        .Select(x => x.CategoryName)
                        .FirstOrDefault()
                })
                .ToList();


            // 22. Rank products by revenue (1, 2, 3 ...) and return Rank, Product, Revenue
            var productRevenues = db.Products
                .Select(p => new {
                    Product = p.Name,
                    Revenue = p.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                        .Sum(oi => (decimal?)(oi.Quantity * oi.UnitPrice * (1 - oi.Discount))) ?? 0
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var q22 = productRevenues
                .Select((x, index) => new {
                    Rank = index + 1,
                    x.Product,
                    x.Revenue
                })
                .ToList();

            // ==========================================
            // Level 4: Real-world methods
            // ==========================================

            // 23. Write SearchProducts(ProductFilter f).

            var filter = new ProductFilter
            {
                SearchText = "Laptop",
                MinPrice = 100,
                InStockOnly = true,
                SortBy = "price",
                SortDescending = true,
                PageNumber = 1,
                PageSize = 5
            };
           
            var productService = new ProductService(db);
            
            // استدعاء ميثود البحث
            var result = productService.SearchProducts(filter);

            // طباعة النتائج
            Console.WriteLine($"Total Found: {result.TotalCount}");
            foreach (var product in result.Items)
            {
                Console.WriteLine($"Product: {product.Name}, Price: {product.Price}");
            }

            // 24 & 25
            var service = new StoreManagementService(db);

            // تشغيل السؤال 24 (تحديث الأسعار في قاعدة البيانات مباشرة)
            int updatedRows = service.IncreaseAccessoryPrices();
            Console.WriteLine($"Updated products count: {updatedRows}");

            // تشغيل السؤال 25 (حذف الطلبات القديمة مباشرة)
            int deletedRows = service.DeleteOldCancelledOrders();
            Console.WriteLine($"Deleted cancelled orders count: {deletedRows}");

            // ==========================================
            // Bonus
            // ==========================================

            // (a) Print the SQL generated for question 8 using ToQueryString().
            var query = db.Orders
                 .Where(o => o.OrderDate.Year == 2025)
                 .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                 .Select(g => new {
                     Year = g.Key.Year,
                     Month = g.Key.Month,
                     OrderCount = g.Count(),
                     Revenue = g.Where(o => o.Status == OrderStatus.Delivered)
                                  .SelectMany(o => o.Items)
                                  .Sum(i => i.Quantity * i.UnitPrice * (1 - i.Discount))
                 });

            // طباعة كود الـ SQL الناتج في الـ Console
            string generatedSql = query.ToQueryString();
            Console.WriteLine(generatedSql);

            //(b) Explain the N+1 problem and show where it would appear if question 20 were  written with a foreach loop.
            /* What is the N + 1 problem?
             It is a performance anti - pattern where an application executes one initial query to fetch a list of parent records($1$),
             and then executes a separate query for each of the retrieved items($N$ times), resulting in $N + 1$ total database round trips.
             Where it would appear in question 20:   
             If question 20 were implemented by first fetching all customers with one query,
            and then looping through them using a foreach statement to fetch the most recent order and its items for each customer individually via separate database calls,
             the N + 1 problem would occur.*/


            //(c) Why can't Select((x, i) => ...) (with an index) be translated to SQL in question 22, and what is the fix ?
             /* Why it cannot be translated to SQL:
                Relational databases do not inherently guarantee a fixed, 
                deterministic row order in underlying tables during SQL translation unless an explicit ORDER BY clause is used.
                Furthermore, EF Core cannot translate the C# sequence index (i) into a standard,
                universally supported SQL clause across all database engines.   The Fix:
                The workaround is to fetch and order the data from the database into memory first using .ToList(), and then apply the indexed Select((x, index) => ...) operation client-side in C# memory.  

            */




}
}
}