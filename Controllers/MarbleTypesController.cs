// Controllers/MarbleTypesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers
{
    public class MarbleTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarbleTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var marbleTypes = await _context.MarbleTypes.ToListAsync();
            return View(marbleTypes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarbleType marbleType)
        {
            if (ModelState.IsValid)
            {
                marbleType.Id = Guid.NewGuid().ToString();
                marbleType.CreatedDate = DateTime.Now;
                _context.Add(marbleType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Marble type created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(marbleType);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var marbleType = await _context.MarbleTypes.FindAsync(id);
            if (marbleType == null) return NotFound();
            
            return View(marbleType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, MarbleType marbleType)
        {
            if (id != marbleType.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(marbleType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Marble type updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MarbleTypeExists(marbleType.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(marbleType);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var marbleType = await _context.MarbleTypes
                .Include(mt => mt.Slabs)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (marbleType == null) return NotFound();
            
            return View(marbleType);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            
            var marbleType = await _context.MarbleTypes.FindAsync(id);
            if (marbleType == null) return NotFound();
            
            return View(marbleType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var marbleType = await _context.MarbleTypes.FindAsync(id);
            if (marbleType != null)
            {
                marbleType.IsActive = false; // Soft delete
                _context.Update(marbleType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Marble type deactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MarbleTypeExists(string id)
        {
            return _context.MarbleTypes.Any(e => e.Id == id);
        }
    }
}