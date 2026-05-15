using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class ResendVerificationDto
    {
        [Required]
        public string Username { get; set; } = "";
    }
}