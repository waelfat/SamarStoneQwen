// Models/Payment.cs
namespace SamarStoneQwen.Models
{
    public class Payment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PurchaseOrderId { get; set; } = string.Empty;
        public decimal AmountUSD { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // MoneyTransfer
        public DateTime PaymentDate { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  PurchaseOrder PurchaseOrder { get; set; } 
    }
}