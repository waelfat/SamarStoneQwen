using System;

namespace SamarStoneQwen.ViewModels.UserManagement;

    public class ResetPasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    
}