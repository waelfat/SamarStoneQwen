// Models/SaleItem.cs
namespace SamarStoneQwen.Models
{
    public class SaleItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SaleId { get; set; } = string.Empty;
        public string SlabId { get; set; } = string.Empty;
        public decimal AreaSold { get; set; } // in square meters
        public decimal SellingPriceEGP { get; set; }
        public decimal CostPerSqM { get; set; } // Cost per square meter in USD
        public decimal Profit => SellingPriceEGP - (AreaSold * CostPerSqM * 15.7m); // USD to EGP conversion
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public  Sale Sale { get; set; } = new Sale();
        public  Slab Slab { get; set; } = new Slab();
    }
}