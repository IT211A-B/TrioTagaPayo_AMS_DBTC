using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;
using BCrypt.Net;

namespace Attendance_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher hasher,
            IJwtTokenGenerator tokenGenerator,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("[LOGIN] Attempt for user: {Username}", request.Username);

                var user = await _userRepository.FindAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    _logger.LogWarning("[LOGIN] User not found: {Username}", request.Username);
                    return null;
                }

                _logger.LogInformation("[LOGIN] User found: {Username}, Role: {Role}, HashLength: {Length}",
                    user.Username, user.Role, user.PasswordHash?.Length ?? 0);

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogError("[LOGIN] Password hash is null for user: {Username}", request.Username);
                    return null;
                }

                _logger.LogInformation("[LOGIN] Verifying password with BCrypt...");

                bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                _logger.LogInformation("[LOGIN] BCrypt verification result: {Result}", passwordValid);

                if (!passwordValid)
                {
                    _logger.LogWarning("[LOGIN] Invalid password for user: {Username}", request.Username);
                    return null;
                }

                _logger.LogInformation("[LOGIN] Password verified successfully");

                _logger.LogInformation("[LOGIN] Generating JWT token for: {Username}", request.Username);
                var token = _tokenGenerator.Generate(user);
                var expiration = DateTime.UtcNow.AddHours(8);

                _logger.LogInformation("[LOGIN] Generating refresh token");
                var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var refreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = refreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("[LOGIN] SUCCESS for user: {Username}", request.Username);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LOGIN] CRITICAL ERROR for user: {Username}", request.Username);
                throw;
            }
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("[REFRESH] Attempt with token");

                var user = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken);

                if (user == null)
                {
                    _logger.LogWarning("[REFRESH] No user found with this refresh token");
                    return null;
                }

                if (user.RefreshTokenExpiry == null)
                {
                    _logger.LogWarning("[REFRESH] Refresh token expiry is null for user: {Username}", user.Username);
                    return null;
                }

                if (user.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("[REFRESH] Refresh token expired for user: {Username}", user.Username);
                    return null;
                }

                _logger.LogInformation("[REFRESH] Valid refresh token for user: {Username}", user.Username);

                var newAccessToken = _tokenGenerator.Generate(user);
                var newExpiration = DateTime.UtcNow.AddHours(8);
                var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var newRefreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = newRefreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("[REFRESH] SUCCESS for user: {Username}", user.Username);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REFRESH] CRITICAL ERROR");
                throw;
            }
        }

        public async Task SeedAdminAsync()
        {
            try
            {
                _logger.LogInformation("[SEED] Checking if admin exists...");

                var adminExists = await _userRepository.AnyAsync(u => u.Role == "Admin");

                if (adminExists)
                {
                    _logger.LogInformation("[SEED] Admin already exists, skipping seed");
                    return;
                }

                _logger.LogInformation("[SEED] Creating admin user...");

                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = _hasher.Hash("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(admin);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("[SEED] Admin created successfully");
                _logger.LogInformation("   Username: admin");
                _logger.LogInformation("   Password: admin123");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEED] ERROR creating admin");
                throw;
            }
        }
    }
}