// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace SamarStoneQwen.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
                public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}