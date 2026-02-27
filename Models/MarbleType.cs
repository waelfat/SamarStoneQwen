// Models/MarbleType.cs
// Models/MarbleType.cs 
using SamarStoneQwen.Models;

public class MarbleType
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = string.Empty;
        public decimal DefaultCostPerSqM { get; set; } // Default cost in USD
        public string QualityGrade { get; set; } = string.Empty; // A, B, C
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public  ICollection<Slab> Slabs { get; set; } = new List<Slab>();
    }
