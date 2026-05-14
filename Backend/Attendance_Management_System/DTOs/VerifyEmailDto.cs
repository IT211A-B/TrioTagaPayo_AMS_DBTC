using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class VerifyEmailDto
    {
        [Required]
        public string Token { get; set; } = "";
    }
}