// Controllers/ReportsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> InventoryReport()
        {
            var report = new InventoryReportViewModel
            {
                TotalSlabs = await _context.Slabs.CountAsync(),
                AvailableSlabs = await _context.Slabs.CountAsync(s => s.Status == "Available"),
                ReservedSlabs = await _context.Slabs.CountAsync(s => s.Status == "Reserved"),
                SoldSlabs = await _context.Slabs.CountAsync(s => s.Status == "Sold"),
                TotalArea = await _context.Slabs.SumAsync(s => s.Area),
                TotalValueUSD = await _context.Slabs.SumAsync(s => s.TotalCost)
            };

            var marbleTypes = await _context.Slabs
                .GroupBy(s => s.MarbleTypeNavigation.Name)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    TotalArea = g.Sum(s => s.Area),
                    TotalValue = g.Sum(s => s.TotalCost),
                    Available = g.Count(s => s.Status == "Available"),
                    Sold = g.Count(s => s.Status == "Sold")
                })
                .ToListAsync();

            report.MarbleTypeBreakdown = marbleTypes;

            return View(report);
        }

        public async Task<IActionResult> FinancialReport()
        {
            var report = new FinancialReportViewModel
            {
                TotalRevenue = await _context.SaleItems.SumAsync(si => si.SellingPriceEGP),
                TotalCostUSD = await _context.SaleItems.SumAsync(si => si.AreaSold * si.CostPerSqM),
                TotalProfit = await _context.SaleItems.SumAsync(si => si.SellingPriceEGP - (si.AreaSold * si.CostPerSqM * 15.7m)),
                TotalExpenses = await _context.Payments.SumAsync(p => p.AmountUSD),
                TotalSalesCount = await _context.Sales.CountAsync(),
                TotalSaleItems = await _context.SaleItems.CountAsync()
            };

            // Convert costs to EGP for comparison
            report.TotalCostEGP = report.TotalCostUSD * 15.7m; // USD to EGP conversion rate

            return View(report);
        }

        public async Task<IActionResult> ProfitabilityByMarbleType()
        {
            var report = await _context.SaleItems
                .Include(si => si.Slab)
                .GroupBy(si => si.Slab.MarbleTypeNavigation.Name)
                .Select(g => new ProfitabilityReportItem
                {
                    MarbleType = g.Key,
                    SalesCount = g.Count(),
                    TotalRevenue = g.Sum(si => si.SellingPriceEGP),
                    TotalCost = g.Sum(si => si.AreaSold * si.CostPerSqM) * 15.7m, // Convert USD to EGP
                    TotalProfit = g.Sum(si => si.SellingPriceEGP - (si.AreaSold * si.CostPerSqM * 15.7m)), // Calculate profit directly
                    AverageSellingPrice = g.Average(si => si.SellingPriceEGP),
                    ProfitMargin = g.Any() ? g.Sum(si => si.SellingPriceEGP - (si.AreaSold * si.CostPerSqM * 15.7m)) / g.Sum(si => si.SellingPriceEGP) * 100 : 0
                })
                .OrderByDescending(r => r.TotalProfit)
                .ToListAsync();

            return View(report);
        }

        public async Task<IActionResult> SalesByCustomer()
        {
            var report = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .GroupBy(s => s.Customer.Name)
                .Select(g => new SalesByCustomerReportItem
                {
                    CustomerName = g.Key,
                    TotalSales = g.Count(),
                    TotalRevenue = g.Sum(s => s.SaleItems.Sum(si => si.SellingPriceEGP)),
                    TotalProfit = g.Sum(s => s.SaleItems.Sum(si => si.SellingPriceEGP - (si.AreaSold * si.CostPerSqM * 15.7m))) // Calculate profit directly
                })
                .OrderByDescending(r => r.TotalRevenue)
                .ToListAsync();

            return View(report);
        }
    }

    public class InventoryReportViewModel
    {
        public int TotalSlabs { get; set; }
        public int AvailableSlabs { get; set; }
        public int ReservedSlabs { get; set; }
        public int SoldSlabs { get; set; }
        public decimal TotalArea { get; set; }
        public decimal TotalValueUSD { get; set; }
        public IEnumerable<dynamic> MarbleTypeBreakdown { get; set; }
    }

    public class FinancialReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCostUSD { get; set; }
        public decimal TotalCostEGP { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalExpenses { get; set; }
        public int TotalSalesCount { get; set; }
        public int TotalSaleItems { get; set; }
    }

    public class ProfitabilityReportItem
    {
        public string MarbleType { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal AverageSellingPrice { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class SalesByCustomerReportItem
    {
        public string CustomerName { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }
}