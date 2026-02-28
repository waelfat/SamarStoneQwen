// ViewModels/Sales/CreateSaleViewModel.cs
using SamarStoneQwen.Models;

namespace SamarStoneQwen.ViewModels.Sales
{
    public class CreateSaleViewModel
    {
        public string CustomerId { get; set; } = string.Empty;
        
        // List of sale items instead of separate lists
        public List<SaleItemViewModel> SaleItems { get; set; } = new List<SaleItemViewModel>();
        
        // Navigation properties for display
        public IEnumerable<Customer> AvailableCustomers { get; set; } = new List<Customer>();
        public IEnumerable<MarbleType> AvailableMarbleTypes { get; set; } = new List<MarbleType>();
    }
}