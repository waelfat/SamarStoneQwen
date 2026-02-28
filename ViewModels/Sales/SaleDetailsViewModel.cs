// ViewModels/Sales/SaleDetailsViewModel.cs
namespace SamarStoneQwen.ViewModels.Sales;

    public class SaleDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Financial summary
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        
        // Sale items
        public List<SaleItemDetailViewModel> SaleItems { get; set; } = new List<SaleItemDetailViewModel>();
    }

    public class SaleItemDetailViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string SlabId { get; set; } = string.Empty;
        public string SlabNumber { get; set; } = string.Empty;
        public string MarbleTypeName { get; set; } = string.Empty;
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Thickness { get; set; }
        public decimal AreaSold { get; set; }
        public decimal CostPerSqM { get; set; }
        public decimal SellingPriceEGP { get; set; }
        
        // Calculated properties
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
