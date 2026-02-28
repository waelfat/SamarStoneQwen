// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace SamarStoneQwen.Models;
// Models/ApplicationUser.cs
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
