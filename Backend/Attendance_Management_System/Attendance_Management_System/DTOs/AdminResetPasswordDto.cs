using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class AdminResetPasswordDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = "";
    }
}