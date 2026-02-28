// Controllers/CustomersController.cs (Updated with ViewModels)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;
using SamarStoneQwen.ViewModels.Customers;

namespace SamarStoneQwen.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View(new CreateCustomerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    TaxNumber = model.TaxNumber,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    Notes = model.Notes,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.Now
                };

                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            
            var model = new EditCustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                TaxNumber = customer.TaxNumber,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                Notes = customer.Notes,
                IsActive = customer.IsActive
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditCustomerViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var customer = await _context.Customers.FindAsync(id);
                    if (customer == null) return NotFound();

                    customer.Name = model.Name;
                    customer.Email = model.Email;
                    customer.Phone = model.Phone;
                    customer.TaxNumber = model.TaxNumber;
                    customer.Address = model.Address;
                    customer.City = model.City;
                    customer.Country = model.Country;
                    customer.Notes = model.Notes;
                    customer.IsActive = model.IsActive;

                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            
            var customer = await _context.Customers
                .Include(c => c.Sales)
                .ThenInclude(s => s.SaleItems)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (customer == null) return NotFound();
            
            return View(customer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            var model = new DeleteCustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                IsActive = customer.IsActive,
                RelatedSalesCount = await _context.Sales.CountAsync(s => s.CustomerId == id)
            };
            
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // Instead of hard delete, mark as inactive
                customer.IsActive = false;
                _context.Update(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer deactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}