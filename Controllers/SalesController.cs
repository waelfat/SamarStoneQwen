// Controllers/SalesController.cs (Fixed)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers
{
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Slab)
                .ThenInclude(s => s.MarbleTypeNavigation)
                .ToListAsync();
            return View(sales);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
            
            // Get available marble types only
            var availableMarbleTypes = await _context.MarbleTypes
                .Where(mt => mt.IsActive && _context.Slabs.Any(s => s.MarbleTypeNavigation.Id == mt.Id && s.Status == "Available"))
                .Select(mt => mt.Name)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
                
            ViewBag.AvailableMarbleTypes = availableMarbleTypes;
            
            return View(new Sale());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId")] Sale sale, 
            List<string> marbleTypes, List<decimal> widths, List<decimal> lengths, 
            List<decimal> thicknesses, List<int> quantities, List<decimal> unitPrices)
        {
            if (ModelState.IsValid && marbleTypes != null && widths != null && lengths != null && 
                thicknesses != null && quantities != null && unitPrices != null)
            {
                // Validate that all arrays have the same length
                int expectedCount = marbleTypes.Count;
                if (widths.Count != expectedCount || lengths.Count != expectedCount || 
                    thicknesses.Count != expectedCount || quantities.Count != expectedCount || 
                    unitPrices.Count != expectedCount)
                {
                    ModelState.AddModelError("", "All input arrays must have the same length.");
                    return await LoadViewForCreate();
                }

                // Validate quantities are positive
                if (quantities.Any(q => q <= 0))
                {
                    ModelState.AddModelError("", "Quantities must be greater than zero.");
                    return await LoadViewForCreate();
                }

                // Validate dimensions are positive
                if (widths.Any(w => w <= 0) || lengths.Any(l => l <= 0) || thicknesses.Any(t => t <= 0))
                {
                    ModelState.AddModelError("", "Dimensions must be greater than zero.");
                    return await LoadViewForCreate();
                }

                // Validate unit prices are positive
                if (unitPrices.Any(up => up <= 0))
                {
                    ModelState.AddModelError("", "Unit prices must be greater than zero.");
                    return await LoadViewForCreate();
                }

                // Find matching slabs for each requirement
                var allMatchingSlabs = new List<(List<Slab> slabs, decimal unitPrice)>();
                
                for (int i = 0; i < marbleTypes.Count; i++)
                {
                    var matchingSlabs = await FindMatchingSlabsFIFO(
                        marbleTypes[i], 
                        widths[i], 
                        lengths[i], 
                        thicknesses[i], 
                        quantities[i]
                    );

                    if (matchingSlabs.Count != quantities[i])
                    {
                        // Not enough matching slabs available
                        ModelState.AddModelError("", 
                            $"Insufficient inventory for {marbleTypes[i]} - requested {quantities[i]}, available: {matchingSlabs.Count}. " +
                            $"Please adjust your requirements.");
                        return await LoadViewForCreate();
                    }

                    allMatchingSlabs.Add((matchingSlabs, unitPrices[i]));
                }

                // If we reach here, all requirements can be fulfilled
                sale.Id = Guid.NewGuid().ToString();
                sale.SaleDate = DateTime.Now;
                sale.CreatedDate = DateTime.Now;

                _context.Add(sale);
                await _context.SaveChangesAsync();

                // Create sale items and update slab statuses
                foreach (var (slabs, unitPrice) in allMatchingSlabs)
                {
                    foreach (var slab in slabs)
                    {
                        var saleItem = new SaleItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            SaleId = sale.Id,
                            SlabId = slab.Id,
                            AreaSold = slab.Area,
                            SellingPriceEGP = unitPrice,
                            CostPerSqM = slab.CostPerSqM
                        };
                        
                        _context.Add(saleItem);

                        // Update slab status to sold
                        slab.Status = "Sold";
                        _context.Update(slab);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return await LoadViewForCreate();
        }

        // Method to find matching slabs using FIFO - Updated to use marble type name
        private async Task<List<Slab>> FindMatchingSlabsFIFO(string marbleTypeName, decimal minWidth, decimal minLength, 
            decimal minThickness, int quantity)
        {
            var matchingSlabs = await _context.Slabs
                .Include(s => s.MarbleTypeNavigation)
                .Where(s => s.Status == "Available" && 
                           s.MarbleTypeNavigation.Name == marbleTypeName &&
                           s.Width >= minWidth &&
                           s.Length >= minLength &&
                           s.Thickness >= minThickness)
                .OrderBy(s => s.CreatedDate) // FIFO: Oldest first
                .Take(quantity)
                .ToListAsync();

            return matchingSlabs;
        }

        private async Task<IActionResult> LoadViewForCreate()
        {
            ViewBag.Customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
            
            // Get available marble types only
            var availableMarbleTypes = await _context.MarbleTypes
                .Where(mt => mt.IsActive && _context.Slabs.Any(s => s.MarbleTypeNavigation.Id == mt.Id && s.Status == "Available"))
                .Select(mt => mt.Name)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
                
            ViewBag.AvailableMarbleTypes = availableMarbleTypes;
            
            return View(new Sale());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Slab)
                .ThenInclude(s => s.MarbleTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (sale == null) return NotFound();
            
            return View(sale);
        }
    }
}