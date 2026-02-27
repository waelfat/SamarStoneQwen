// Controllers/SuppliersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return View(suppliers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.Id = Guid.NewGuid().ToString();
                supplier.CreatedDate = DateTime.Now;
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Supplier created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Supplier updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var supplier = await _context.Suppliers
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (supplier == null) return NotFound();
            
            return View(supplier);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            
            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                supplier.IsActive = false; // Soft delete
                _context.Update(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Supplier deactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(string id)
        {
            return _context.Suppliers.Any(e => e.Id == id);
        }
    }
}