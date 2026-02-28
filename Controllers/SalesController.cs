// Controllers/SalesController.cs (Updated Details method)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;
using SamarStoneQwen.ViewModels.Sales;

namespace SamarStoneQwen.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
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
                .ToListAsync();
            return View(sales);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateSaleViewModel
            {
                AvailableCustomers = await _context.Customers.Where(c => c.IsActive).ToListAsync(),
                AvailableMarbleTypes = await _context.MarbleTypes
                    .Where(mt => mt.IsActive && _context.Slabs.Any(s => s.MarbleTypeNavigation.Id == mt.Id && s.Status == "Available"))
                    .ToListAsync()
            };

            // Initialize with one empty item
            viewModel.SaleItems.Add(new SaleItemViewModel());

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSaleViewModel viewModel)
        {
            if (viewModel.SaleItems == null || !viewModel.SaleItems.Any())
            {
                ModelState.AddModelError("", "At least one sale item must be added.");
                return await LoadViewForCreate();
            }

            // Validate each sale item
            for (int i = 0; i < viewModel.SaleItems.Count; i++)
            {
                var item = viewModel.SaleItems[i];
                
                // Validate quantities are positive
                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError($"SaleItems[{i}].Quantity", "Quantity must be greater than zero.");
                }

                // Validate dimensions are positive
                if (item.Width <= 0 || item.Length <= 0 || item.Thickness <= 0)
                {
                    if (item.Width <= 0) ModelState.AddModelError($"SaleItems[{i}].Width", "Width must be greater than zero.");
                    if (item.Length <= 0) ModelState.AddModelError($"SaleItems[{i}].Length", "Length must be greater than zero.");
                    if (item.Thickness <= 0) ModelState.AddModelError($"SaleItems[{i}].Thickness", "Thickness must be greater than zero.");
                }

                // Validate unit price is positive
                if (item.UnitPrice <= 0)
                {
                    ModelState.AddModelError($"SaleItems[{i}].UnitPrice", "Unit price must be greater than zero.");
                }

                // Validate marble type exists and is active
                if (!string.IsNullOrEmpty(item.MarbleTypeId))
                {
                    var marbleType = await _context.MarbleTypes.FindAsync(item.MarbleTypeId);
                    if (marbleType == null || !marbleType.IsActive)
                    {
                        ModelState.AddModelError($"SaleItems[{i}].MarbleTypeId", "Selected marble type is not available.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return await LoadViewForCreate();
            }

            // Validate all requirements can be fulfilled before creating the sale
            var allMatchingSlabs = new List<(List<Slab> slabs, decimal unitPrice)>();
            
            for (int i = 0; i < viewModel.SaleItems.Count; i++)
            {
                var item = viewModel.SaleItems[i];
                
                var matchingSlabs = await FindMatchingSlabsFIFO(
                    item.MarbleTypeId, 
                    item.Width, 
                    item.Length, 
                    item.Thickness, 
                    item.Quantity
                );

                if (matchingSlabs.Count != item.Quantity)
                {
                    // Not enough matching slabs available
                    var marbleType = await _context.MarbleTypes.FindAsync(item.MarbleTypeId);
                    ModelState.AddModelError("", 
                        $"Insufficient inventory for {marbleType?.Name ?? "Unknown Type"} - requested {item.Quantity}, available: {matchingSlabs.Count}. " +
                        $"Please adjust your requirements.");
                    return await LoadViewForCreate();
                }

                allMatchingSlabs.Add((matchingSlabs, item.UnitPrice));
            }

            // If we reach here, all requirements can be fulfilled
            var sale = new Sale
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = viewModel.CustomerId,
                SaleDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            _context.Add(sale);
            await _context.SaveChangesAsync(); // Save sale first to get ID

            // Create all sale items and update slab statuses in a single operation
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

            await _context.SaveChangesAsync(); // Save all changes together
            TempData["SuccessMessage"] = "Sale created successfully!";
            return RedirectToAction(nameof(Index));
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
            
            // Map to ViewModel
            var viewModel = new SaleDetailsViewModel
            {
                Id = sale.Id,
                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer.Name,
                SaleDate = sale.SaleDate,
                CreatedDate = sale.CreatedDate,
                Notes = sale.Notes
            };

            // Calculate financial summary
            decimal totalRevenue = 0;
            decimal totalCost = 0;
            
            foreach (var item in sale.SaleItems)
            {
                var cost = item.AreaSold * item.CostPerSqM * 15.7m; // USD to EGP conversion
                var profit = item.SellingPriceEGP - cost;
                var profitMargin = item.SellingPriceEGP > 0 ? (profit / item.SellingPriceEGP * 100) : 0;
                
                totalRevenue += item.SellingPriceEGP;
                totalCost += cost;
                
                viewModel.SaleItems.Add(new SaleItemDetailViewModel
                {
                    Id = item.Id,
                    SlabId = item.SlabId,
                    SlabNumber = item.Slab.SlabNumber,
                    MarbleTypeName = item.Slab.MarbleTypeNavigation.Name,
                    Width = item.Slab.Width,
                    Length = item.Slab.Length,
                    Thickness = item.Slab.Thickness,
                    AreaSold = item.AreaSold,
                    CostPerSqM = item.CostPerSqM,
                    SellingPriceEGP = item.SellingPriceEGP,
                    Cost = cost,
                    Profit = profit,
                    ProfitMargin = profitMargin
                });
            }
            
            viewModel.TotalRevenue = totalRevenue;
            viewModel.TotalCost = totalCost;
            viewModel.TotalProfit = totalRevenue - totalCost;
            viewModel.ProfitMargin = totalRevenue > 0 ? (viewModel.TotalProfit / totalRevenue * 100) : 0;

            return View(viewModel);
        }

        // Method to find matching slabs using FIFO
        private async Task<List<Slab>> FindMatchingSlabsFIFO(string marbleTypeId, decimal minWidth, decimal minLength, 
            decimal minThickness, int quantity)
        {
            var matchingSlabs = await _context.Slabs
                .Include(s => s.MarbleTypeNavigation)
                .Where(s => s.Status == "Available" && 
                           s.MarbleTypeId == marbleTypeId &&
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
            var viewModel = new CreateSaleViewModel
            {
                CustomerId = "", // Keep current selection
                SaleItems = new List<SaleItemViewModel>(), // Keep current items
                AvailableCustomers = await _context.Customers.Where(c => c.IsActive).ToListAsync(),
                AvailableMarbleTypes = await _context.MarbleTypes
                    .Where(mt => mt.IsActive && _context.Slabs.Any(s => s.MarbleTypeNavigation.Id == mt.Id && s.Status == "Available"))
                    .ToListAsync()
            };

            // If there are items in the model (from validation error), keep them
            if (ModelState.ErrorCount > 0)
            {
                // Get the current items from the form values
                var customer = Request.Form["CustomerId"].FirstOrDefault();
                viewModel.CustomerId = customer ?? "";

                // Rebuild sale items from form data
                var itemCount = Request.Form["SaleItems.Index"].Count;
                for (int i = 0; i < itemCount; i++)
                {
                    var marbleTypeId = Request.Form[$"SaleItems[{i}].MarbleTypeId"].FirstOrDefault();
                    var widthStr = Request.Form[$"SaleItems[{i}].Width"].FirstOrDefault();
                    var lengthStr = Request.Form[$"SaleItems[{i}].Length"].FirstOrDefault();
                    var thicknessStr = Request.Form[$"SaleItems[{i}].Thickness"].FirstOrDefault();
                    var quantityStr = Request.Form[$"SaleItems[{i}].Quantity"].FirstOrDefault();
                    var unitPriceStr = Request.Form[$"SaleItems[{i}].UnitPrice"].FirstOrDefault();

                    if (decimal.TryParse(widthStr, out decimal width) &&
                        decimal.TryParse(lengthStr, out decimal length) &&
                        decimal.TryParse(thicknessStr, out decimal thickness) &&
                        int.TryParse(quantityStr, out int quantity) &&
                        decimal.TryParse(unitPriceStr, out decimal unitPrice))
                    {
                        viewModel.SaleItems.Add(new SaleItemViewModel
                        {
                            MarbleTypeId = marbleTypeId ?? "",
                            Width = width,
                            Length = length,
                            Thickness = thickness,
                            Quantity = quantity,
                            UnitPrice = unitPrice
                        });
                    }
                }
            }
            else
            {
                // Initialize with one empty item
                viewModel.SaleItems.Add(new SaleItemViewModel());
            }
            
            return View("Create", viewModel);
        }
    }
}