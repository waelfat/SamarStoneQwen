// Updated PurchaseOrder.cs to include Order Number
namespace SamarStoneQwen.Models
{
    public class PurchaseOrder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SupplierId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty; // Add this property
        public DateTime OrderDate { get; set; }
        public decimal TotalAmountUSD { get; set; }
        public decimal PaidAmountUSD { get; set; }
        public decimal RemainingAmountUSD => TotalAmountUSD - PaidAmountUSD;
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime ExpectedDeliveryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  Supplier Supplier { get; set; } = new Supplier();
        public  ICollection<Container> Containers { get; set; } = new List<Container>();
        public  ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}