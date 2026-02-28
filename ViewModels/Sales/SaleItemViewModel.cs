// ViewModels/Sales/SaleItemViewModel.cs
namespace SamarStoneQwen.ViewModels.Sales
{
    public class SaleItemViewModel
    {
        public string MarbleTypeId { get; set; } = string.Empty;
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Thickness { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}