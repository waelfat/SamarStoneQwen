// Updated PurchaseOrdersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Controllers;

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
        // In PurchaseOrdersController.cs, add these methods:
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddPayment([FromBody] PaymentRequestModel request)
{
    if (request == null || string.IsNullOrEmpty(request.PurchaseOrderId))
    {
        return Json(new { success = false, message = "Invalid request." });
    }

    var purchaseOrder = await _context.PurchaseOrders.FindAsync(request.PurchaseOrderId);
    if (purchaseOrder == null)
    {
        return Json(new { success = false, message = "Purchase order not found." });
    }

    // Check if payment amount is valid
    var remainingAmount = purchaseOrder.TotalAmountUSD - purchaseOrder.PaidAmountUSD;
    if (request.AmountUSD > remainingAmount)
    {
        return Json(new { success = false, message = $"Payment amount exceeds remaining balance. Maximum allowed: {remainingAmount:C2}" });
    }

    var payment = new Payment
    {
        Id = Guid.NewGuid().ToString(),
        PurchaseOrder = purchaseOrder,
        AmountUSD = request.AmountUSD,
        PaymentMethod = request.PaymentMethod,
        PaymentDate = request.PaymentDate,
        TransactionReference = string.IsNullOrEmpty(request.TransactionReference) ? 
            $"MT-{DateTime.Now:yyyyMMdd-HHmmss}" : request.TransactionReference,
        Description = request.Description,
        CreatedDate = DateTime.Now
    };

    _context.Payments.Add(payment);
    
    // Update purchase order paid amount
    purchaseOrder.PaidAmountUSD += request.AmountUSD;
    
    // Update status
    if (purchaseOrder.PaidAmountUSD >= purchaseOrder.TotalAmountUSD)
    {
        purchaseOrder.Status = "Paid";
    }
    else if (purchaseOrder.PaidAmountUSD > 0)
    {
        purchaseOrder.Status = "PartiallyPaid";
    }

    await _context.SaveChangesAsync();

    return Json(new { 
        success = true, 
        message = "Payment added successfully!", 
        payment = new { 
            id = payment.Id,
            paymentDate = payment.PaymentDate.ToString("yyyy-MM-dd"),
            amountUSD = payment.AmountUSD,
            paymentMethod = payment.PaymentMethod,
            transactionReference = payment.TransactionReference,
            description = payment.Description
        }
    });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeletePayment(string id)
{
    if (string.IsNullOrEmpty(id))
    {
        return Json(new { success = false, message = "Invalid payment ID." });
    }

    var payment = await _context.Payments.FindAsync(id);
    if (payment == null)
    {
        return Json(new { success = false, message = "Payment not found." });
    }

    var purchaseOrder = await _context.PurchaseOrders.FindAsync(payment.PurchaseOrderId);
    if (purchaseOrder != null)
    {
        // Update purchase order paid amount
        purchaseOrder.PaidAmountUSD -= payment.AmountUSD;
        
        // Update status
        if (purchaseOrder.PaidAmountUSD <= 0)
        {
            purchaseOrder.Status = "Pending";
        }
        else if (purchaseOrder.PaidAmountUSD < purchaseOrder.TotalAmountUSD)
        {
            purchaseOrder.Status = "PartiallyPaid";
        }
    }

    _context.Payments.Remove(payment);
    await _context.SaveChangesAsync();

    return Json(new { success = true, message = "Payment deleted successfully!" });
}

// Model for payment requests
public class PaymentRequestModel
{
    public string PurchaseOrderId { get; set; } = string.Empty;
    public decimal AmountUSD { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string TransactionReference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
    }
