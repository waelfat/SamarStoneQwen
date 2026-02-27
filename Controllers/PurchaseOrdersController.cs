// Updated PurchaseOrdersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .ToListAsync();
            return View(purchaseOrders);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(new PurchaseOrder());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrder purchaseOrder)
        {
            if (ModelState.IsValid)
            {
                purchaseOrder.Id = Guid.NewGuid().ToString();
                purchaseOrder.CreatedDate = DateTime.Now;
                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(purchaseOrder);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null) return NotFound();
            
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(purchaseOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrder.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(purchaseOrder);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Containers)
                .Include(po => po.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (purchaseOrder == null) return NotFound();
            
            return View(purchaseOrder);
        }

        private bool PurchaseOrderExists(string id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }
    }
}