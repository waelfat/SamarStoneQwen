// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using SamarStoneQwen.Data;
using Microsoft.EntityFrameworkCore;

namespace SamarStoneQwen.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalPurchaseOrders = await _context.PurchaseOrders.CountAsync(),
                TotalContainers = await _context.Containers.CountAsync(),
                TotalSlabs = await _context.Slabs.CountAsync(),
                TotalSales = await _context.Sales.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalRevenue = await _context.SaleItems.SumAsync(si => si.SellingPriceEGP),
                TotalProfit = await _context.SaleItems.SumAsync(si => si.SellingPriceEGP - (si.AreaSold * si.CostPerSqM * 15.7m)) // Calculate profit directly
            };

            return View(model);
        }
    }

    public class DashboardViewModel
    {
        public int TotalPurchaseOrders { get; set; }
        public int TotalContainers { get; set; }
        public int TotalSlabs { get; set; }
        public int TotalSales { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }
}