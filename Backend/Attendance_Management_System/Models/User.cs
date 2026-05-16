using Attendance_Management_System.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public int? TeacherId { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // ✅ NEW: Link to Student (for Student role)
    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    // Email verification fields
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
}