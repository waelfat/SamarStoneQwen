// Controllers/ContainersController.cs (Fixed)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;
using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace SamarStoneQwen.Controllers
{
    public class ContainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var containers = await _context.Containers
                .Include(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .ToListAsync();
            return View(containers);
        }

        public async Task<IActionResult> Create(string purchaseOrderId)
        {
            ViewBag.PurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => po.Status != "Delivered")
                .ToListAsync();
           
            var container = new Container { PurchaseOrderId = purchaseOrderId };
            return View(container);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Container container)
        {
            if (ModelState.IsValid)
            {
                container.Id = Guid.NewGuid().ToString();
                container.CreatedDate = DateTime.Now;
                _context.Add(container);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "PurchaseOrders", new { id = container.PurchaseOrderId });
            }
            ViewBag.PurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => po.Status != "Delivered")
                .ToListAsync();
            return View(container);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var container = await _context.Containers
                .Include(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (container == null) return NotFound();
            
            ViewBag.PurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => po.Status != "Delivered")
                .ToListAsync();
            return View(container);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Container container)
        {
            if (id != container.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(container);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerExists(container.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PurchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Where(po => po.Status != "Delivered")
                .ToListAsync();
            return View(container);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var container = await _context.Containers
                .Include(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .Include(c => c.Slabs)
                .ThenInclude(s => s.MarbleTypeNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (container == null) return NotFound();
            
            return View(container);
        }

        [HttpGet]
        public async Task<IActionResult> UploadSlabFile(string containerId)
        {
            if (containerId == null) return NotFound();
            
            var container = await _context.Containers
                .Include(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .FirstOrDefaultAsync(c => c.Id == containerId);
            if (container == null) return NotFound();
            
            ViewBag.Container = container;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSlabFile(string containerId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select an Excel file.");
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Only Excel files (.xlsx) are allowed.");
                return View();
            }

            try
            {
                var filePath = Path.Combine("wwwroot/uploads", file.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Process the Excel file and save slabs
                await ProcessSlabFile(filePath, containerId);

                // Delete the uploaded file after processing
                System.IO.File.Delete(filePath);

                TempData["SuccessMessage"] = "Slabs imported successfully!";
                return RedirectToAction("Details", new { id = containerId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View();
            }
        }

        private async Task ProcessSlabFile(string filePath, string containerId)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                
                // Skip header row (assuming first row contains headers)
                var startRow = 2;
                var rowCount = worksheet.RowsUsed().Count();

                for (int row = startRow; row <= rowCount; row++)
                {
                    var slabNumber = worksheet.Cell(row, 1).GetValue<string>() ?? "";
                    var marbleTypeName = worksheet.Cell(row, 2).GetValue<string>() ?? "";
                    var thickness = worksheet.Cell(row, 3).GetValue<decimal>();
                    var width = worksheet.Cell(row, 4).GetValue<decimal>();
                    var length = worksheet.Cell(row, 5).GetValue<decimal>();
                    var qualityGrade = worksheet.Cell(row, 6).GetValue<string>() ?? "A";
                    var color = worksheet.Cell(row, 7).GetValue<string>() ?? "";
                    var originCountry = worksheet.Cell(row, 8).GetValue<string>() ?? "";
                    var costPerSqM = worksheet.Cell(row, 9).GetValue<decimal>();

                    // Find or create marble type
                    var marbleType = await _context.MarbleTypes
                        .FirstOrDefaultAsync(mt => mt.Name == marbleTypeName);
                    
                    if (marbleType == null)
                    {
                        // Create new marble type if not found
                        marbleType = new MarbleType
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = marbleTypeName,
                            Color = color,
                            OriginCountry = originCountry,
                            QualityGrade = qualityGrade,
                            DefaultCostPerSqM = costPerSqM,
                            CreatedDate = DateTime.Now
                        };
                        _context.MarbleTypes.Add(marbleType);
                        await _context.SaveChangesAsync();
                    }

                    var slab = new Slab
                    {
                        Id = Guid.NewGuid().ToString(),
                        ContainerId = containerId,
                        MarbleTypeId = marbleType.Id,
                        SlabNumber = slabNumber,
                        Thickness = thickness,
                        Width = width,
                        Length = length,
                        CostPerSqM = costPerSqM
                    };
                    
                    _context.Slabs.Add(slab);
                }
                
                await _context.SaveChangesAsync();
            }
        }

        private bool ContainerExists(string id)
        {
            return _context.Containers.Any(e => e.Id == id);
        }
    }
}