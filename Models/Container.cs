// Models/Container.cs
namespace SamarStoneQwen.Models
{
    public class Container
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ContainerNumber { get; set; } = string.Empty;
        public string PurchaseOrderId { get; set; } = string.Empty;
        public int SizeInFeet { get; set; } // 20, 40 feet
        public decimal WeightInTons { get; set; }
        public string Status { get; set; } = "Ordered"; // Ordered, Shipped, Arrived, Unloaded
        public DateTime? ArrivalDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  PurchaseOrder PurchaseOrder { get; set; } = new PurchaseOrder();
        public  ICollection<Slab> Slabs { get; set; } = new List<Slab>();
    }
}