namespace Attendance_Management_System.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = "";
        public DateTime RefreshTokenExpiry { get; set; }
        public int? TeacherId { get; set; }
        public string? FullName { get; set; }
    }

    // ✅ NEW — para sa POST /api/Auth/refresh
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}