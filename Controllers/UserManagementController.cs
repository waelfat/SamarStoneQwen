// Controllers/UserManagementController.cs (Simplified)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;
using SamarStoneQwen.ViewModels.Account;
using SamarStoneQwen.ViewModels.UserManagement;

namespace SamarStoneQwen.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateUserViewModel model)
{
    if (ModelState.IsValid)
    {
        // Check if passwords match
       if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return View(model);
        }

        // Validate role selection
        if (model.Role != "Admin" && model.Role != "Employee")
        {
            ModelState.AddModelError("", "Please select a valid role.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            IsActive = model.IsActive,
            CreatedDate = DateTime.Now
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Ensure roles exist
            var roles = new[] { "Admin", "Employee" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            
            // Assign selected role
            await _userManager.AddToRoleAsync(user, model.Role);
            
            // Set the MustChangePassword flag
            user.MustChangePassword = model.MustChangePassword;
            await _userManager.UpdateAsync(user);
            
            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    return View(model);
}
[HttpGet]
public async Task<IActionResult> Edit(string id)
{
    if (id == null) return NotFound();

    var user = await _userManager.FindByIdAsync(id);
    if (user == null) return NotFound();

    // Get current user's role
    var userRoles = await _userManager.GetRolesAsync(user);
    var currentRole = userRoles.FirstOrDefault() ?? "Employee";

    var model = new EditUserViewModel
    {
        Id = user.Id,
        Username = user.UserName ?? "",
        Email = user.Email ?? "",
        FirstName = user.FirstName ?? "",
        LastName = user.LastName ?? "",
        Role = currentRole,
        IsActive = user.IsActive,
        MustChangePassword = user.MustChangePassword
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(EditUserViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.UserName = model.Username;
        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.IsActive = model.IsActive;
        user.MustChangePassword = model.MustChangePassword;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // Update user's role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            
            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    return View(model);
}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivation(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error updating user status.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Reset password to default
            var result = await _userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddPasswordAsync(user, "Password123!");
            }

            if (result.Succeeded)
            {
                // Set MustChangePassword to true after admin reset
                user.MustChangePassword = true;
                await _userManager.UpdateAsync(user);
                
                TempData["SuccessMessage"] = "Password reset successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error resetting password.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    // Reset MustChangePassword flag after successful change
                    user.MustChangePassword = false;
                    await _userManager.UpdateAsync(user);
                    
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }

    // View Models (add these to your ViewModels folder)


}