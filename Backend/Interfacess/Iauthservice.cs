using Attendance_Management_System.Models;

namespace Attendance_Management_System.Interfacess
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task SeedAdminAsync();
        Task<LoginResponse?> RefreshAsync(string refreshToken); // ✅ ADD
    }
}