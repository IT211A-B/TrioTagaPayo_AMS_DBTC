using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = "";
    }
}