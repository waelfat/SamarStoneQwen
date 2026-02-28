using System;

namespace SamarStoneQwen.ViewModels.UserManagement;
// ViewModels/UserManagement/EditUserViewModel.cs
// Add this to your EditUserViewModel class
public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public string ConfirmPassword { get; set; } = string.Empty; 
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
}
