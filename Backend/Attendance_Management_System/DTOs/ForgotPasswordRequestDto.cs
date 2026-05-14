using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        public string Username { get; set; } = "";
    }
}