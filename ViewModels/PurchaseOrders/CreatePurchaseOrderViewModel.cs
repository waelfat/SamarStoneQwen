// ViewModels/PurchaseOrders/CreatePurchaseOrderViewModel.cs (Updated)
using SamarStoneQwen.Models;

namespace SamarStoneQwen.ViewModels.PurchaseOrders;

    public class CreatePurchaseOrderViewModel
    {
        public string SupplierId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmountUSD { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime ExpectedDeliveryDate { get; set; } = DateTime.Now.AddDays(30);
        public string Notes { get; set; } = string.Empty;

        // Navigation properties for display
        public IEnumerable<Supplier> AvailableSuppliers { get; set; } = new List<Supplier>();
    }
