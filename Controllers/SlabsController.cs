// Updated SlabsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers
{
    public class SlabsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SlabsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var slabs = await _context.Slabs
                .Include(s => s.Container)
                .ThenInclude(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .Include(s => s.MarbleTypeNavigation)
                .ToListAsync();
            return View(slabs);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var slab = await _context.Slabs
                .Include(s => s.Container)
                .ThenInclude(c => c.PurchaseOrder)
                .ThenInclude(po => po.Supplier)
                .Include(s => s.MarbleTypeNavigation)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Sale)
                .ThenInclude(s => s.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (slab == null) return NotFound();
            
            return View(slab);
        }

        public async Task<IActionResult> GetSlabDetails(string id)
        {
            if (id == null) return NotFound();
            
            var slab = await _context.Slabs
                .Include(s => s.MarbleTypeNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (slab == null) return NotFound();
            
            var result = new
            {
                slabNumber = slab.SlabNumber,
                marbleType = slab.MarbleTypeNavigation.Name,
                width = slab.Width,
                length = slab.Length,
                thickness = slab.Thickness,
                area = slab.Area,
                qualityGrade = slab.MarbleTypeNavigation.QualityGrade,
                originCountry = slab.MarbleTypeNavigation.OriginCountry
            };
            
            return Json(result);
        }
    }
}