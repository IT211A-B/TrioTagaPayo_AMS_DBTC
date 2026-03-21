using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Hash password using SHA256
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var hashedPassword = HashPassword(request.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == hashedPassword);

            if (user == null) return null;

            var token = GenerateJwtToken(user);
            var expiration = DateTime.UtcNow.AddHours(8);

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                Expiration = expiration
            };
        }

        public async Task SeedAdminAsync()
        {
            // Only seed if no users exist
            if (await _context.Users.AnyAsync()) return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? "DefaultSuperSecretKey123456789012";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "AttendanceSystem",
                audience: _config["Jwt:Audience"] ?? "AttendanceSystem",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}