// Models/Slab.cs
namespace SamarStoneQwen.Models
{
   
    public class Slab
    { 
          public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ContainerId { get; set; } = string.Empty;
        public string MarbleTypeId { get; set; } = string.Empty;
        public string SlabNumber { get; set; } = string.Empty;
        public decimal Thickness { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Area => (Width * Length) / 10000;
        public decimal CostPerSqM { get; set; }
        public decimal TotalCost => Area * CostPerSqM;
        public string Status { get; set; } = "Available";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  Container Container { get; set; } = new Container();
        public  MarbleType MarbleTypeNavigation { get; set; } = new MarbleType();
        public  ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        /*
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ContainerId { get; set; } = string.Empty;
        public string SlabNumber { get; set; } = string.Empty;
        public string MarbleType { get; set; } = string.Empty;
        public decimal Thickness { get; set; } // in cm
        public decimal Width { get; set; } // in cm
        public decimal Length { get; set; } // in cm
        public decimal Area => (Width * Length) / 10000; // in square meters
        public decimal CostPerSqM { get; set; }
        public decimal TotalCost => Area * CostPerSqM;
        public string Status { get; set; } = "Available"; // Available, Reserved, Sold
        public string QualityGrade { get; set; } = string.Empty; // A, B, C
        public string Color { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  Container Container { get; set; } = new Container();
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
*/
    }
    
}