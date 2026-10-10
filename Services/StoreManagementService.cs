using Microsoft.EntityFrameworkCore;
using Online_Store.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Online_Store.Services
{
    public class StoreManagementService
    {
        private readonly OnlineStoreDbContext _db;

        public StoreManagementService(OnlineStoreDbContext db)
        {
            _db = db;
        }

        // 24. Increase the price by 10% for products in "Accessories" with stock below 5, without loading them into memory
        public int IncreaseAccessoryPrices()
        {
            return _db.Products
                .Where(p => p.Category.Name == "Accessories" && p.Stock < 5)
                .ExecuteUpdate(s => s.SetProperty(p => p.Price, p => p.Price * 1.10m));
        }

        // 25. Delete all cancelled orders older than 1 year without loading them into memory
        public int DeleteOldCancelledOrders()
        {
            var oneYearAgo = DateTime.Now.AddYears(-1);

            return _db.Orders
                .Where(o => o.Status == OrderStatus.Cancelled && o.OrderDate < oneYearAgo)
                .ExecuteDelete();
        }
    }
}