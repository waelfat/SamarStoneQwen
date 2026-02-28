// Controllers/PurchaseOrdersController.cs (Updated without PaidAmount)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;
using SamarStoneQwen.ViewModels.PurchaseOrders;

namespace SamarStoneQwen.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var viewModel = new CreatePurchaseOrderViewModel
            {
                AvailableSuppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseOrderViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var purchaseOrder = new PurchaseOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    SupplierId = viewModel.SupplierId,
                    OrderNumber = GenerateOrderNumber(),
                    OrderDate = viewModel.OrderDate,
                    TotalAmountUSD = viewModel.TotalAmountUSD,
                    Currency = viewModel.Currency,
                    Status = viewModel.Status,
                    ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate,
                    Notes = viewModel.Notes,
                    CreatedDate = DateTime.Now
                };

                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Purchase order created successfully!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.AvailableSuppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null) return NotFound();
            
            var viewModel = new CreatePurchaseOrderViewModel
            {
                SupplierId = purchaseOrder.SupplierId,
                OrderDate = purchaseOrder.OrderDate,
                TotalAmountUSD = purchaseOrder.TotalAmountUSD,
                Currency = purchaseOrder.Currency,
                Status = purchaseOrder.Status,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                Notes = purchaseOrder.Notes,
                AvailableSuppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreatePurchaseOrderViewModel viewModel)
        {
            if (id == null) return NotFound();
            
            if (ModelState.IsValid)
            {
                var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
                if (purchaseOrder == null) return NotFound();

                purchaseOrder.SupplierId = viewModel.SupplierId;
                purchaseOrder.OrderDate = viewModel.OrderDate;
                purchaseOrder.TotalAmountUSD = viewModel.TotalAmountUSD;
                purchaseOrder.Currency = viewModel.Currency;
                purchaseOrder.Status = viewModel.Status;
                purchaseOrder.ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate;
                purchaseOrder.Notes = viewModel.Notes;

                try
                {
                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Purchase order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrder.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            viewModel.AvailableSuppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(viewModel);
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
        
        private string GenerateOrderNumber()
        {
            var yearMonth = DateTime.Now.ToString("yyyy-MM");
            var count = _context.PurchaseOrders.Count(po => po.OrderNumber.StartsWith($"PO-{yearMonth}")) + 1;
            return $"PO-{yearMonth}-{count:D3}";
        }
    }
}