using System;

namespace SamarStoneQwen.ViewModels.UserManagement;
// ViewModels/UserManagement/CreateUserViewModel.cs
// Add this to your CreateUserViewModel class
public class CreateUserViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee"; // Default role
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
}