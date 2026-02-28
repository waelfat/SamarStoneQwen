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
        
        [HttpGet]
        public IActionResult DownloadSample()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SlabData");

                // Add headers
                worksheet.Cell(1, 1).Value = "SlabNumber";
                worksheet.Cell(1, 2).Value = "MarbleType";
                worksheet.Cell(1, 3).Value = "Thickness";
                worksheet.Cell(1, 4).Value = "Width";
                worksheet.Cell(1, 5).Value = "Length";
                worksheet.Cell(1, 6).Value = "QualityGrade";
                worksheet.Cell(1, 7).Value = "Color";
                worksheet.Cell(1, 8).Value = "OriginCountry";
                worksheet.Cell(1, 9).Value = "CostPerSqM";

                // Add sample data
                worksheet.Cell(2, 1).Value = "SLB-001";
                worksheet.Cell(2, 2).Value = "Carrara White";
                worksheet.Cell(2, 3).Value = 2.0;
                worksheet.Cell(2, 4).Value = 120.0;
                worksheet.Cell(2, 5).Value = 240.0;
                worksheet.Cell(2, 6).Value = "A";
                worksheet.Cell(2, 7).Value = "White";
                worksheet.Cell(2, 8).Value = "Italy";
                worksheet.Cell(2, 9).Value = 65.0;

                worksheet.Cell(3, 1).Value = "SLB-002";
                worksheet.Cell(3, 2).Value = "Calacatta Gold";
                worksheet.Cell(3, 3).Value = 2.0;
                worksheet.Cell(3, 4).Value = 115.0;
                worksheet.Cell(3, 5).Value = 235.0;
                worksheet.Cell(3, 6).Value = "A";
                worksheet.Cell(3, 7).Value = "White with Golden Veins";
                worksheet.Cell(3, 8).Value = "Italy";
                worksheet.Cell(3, 9).Value = 120.0;

                worksheet.Cell(4, 1).Value = "SLB-003";
                worksheet.Cell(4, 2).Value = "Emperador Light";
                worksheet.Cell(4, 3).Value = 2.0;
                worksheet.Cell(4, 4).Value = 125.0;
                worksheet.Cell(4, 5).Value = 245.0;
                worksheet.Cell(4, 6).Value = "B";
                worksheet.Cell(4, 7).Value = "Beige/Brown";
                worksheet.Cell(4, 8).Value = "Spain";
                worksheet.Cell(4, 9).Value = 45.0;

                // Format headers
                var headerRange = worksheet.Range("A1:I1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Create memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleSlabData.xlsx");
                }
            }
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
        public async Task<IActionResult> UploadSlabFile(string Id)
        {
            if (Id == null) return NotFound();
            
            var container = await _context.Containers
                .Include(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .FirstOrDefaultAsync(c => c.Id == Id);
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