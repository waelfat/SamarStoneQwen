// Models/Sale.cs
namespace SamarStoneQwen.Models
{
    public class Sale
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  Customer Customer { get; set; } 
        public  ICollection<SaleItem> SaleItems { get; set; } =new List<SaleItem>();
    }
}