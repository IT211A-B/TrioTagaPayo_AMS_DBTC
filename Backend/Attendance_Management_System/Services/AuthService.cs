using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher hasher,
            IJwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _tokenGenerator = tokenGenerator;
        }

        // ── LOGIN ─────────────────────────────────────────────────────────
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAndPasswordAsync(
                request.Username, _hasher.Hash(request.Password));

            if (user == null) return null;

            var token = _tokenGenerator.Generate(user);
            var expiration = DateTime.UtcNow.AddHours(8);

            // ✅ Generate refresh token + save to DB
            var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshExpiry;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                Expiration = expiration,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshExpiry
            };
        }

        // ── REFRESH ───────────────────────────────────────────────────────
        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            var user = await _userRepository.FindAsync(u =>
                u.RefreshToken == refreshToken);

            if (user == null) return null;
            if (user.RefreshTokenExpiry == null) return null;
            if (user.RefreshTokenExpiry < DateTime.UtcNow) return null;

            // ✅ Rotate — new access token + new refresh token
            var newAccessToken = _tokenGenerator.Generate(user);
            var newExpiration = DateTime.UtcNow.AddHours(8);
            var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
            var newRefreshExpiry = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = newRefreshExpiry;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new LoginResponse
            {
                Token = newAccessToken,
                Username = user.Username,
                Role = user.Role,
                Expiration = newExpiration,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiry = newRefreshExpiry
            };
        }

        // ── SEED ADMIN ────────────────────────────────────────────────────
        public async Task SeedAdminAsync()
        {
            if (await _userRepository.AnyAsync(_ => true)) return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = _hasher.Hash("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(admin);
            await _userRepository.SaveChangesAsync();
        }
    }
}