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

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                Console.WriteLine($"[LOGIN] Attempt for user: {request.Username}");

                var user = await _userRepository.FindAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    Console.WriteLine($"[LOGIN] User not found: {request.Username}");
                    return null;
                }

                Console.WriteLine($"[LOGIN] User found: {user.Username}");
                Console.WriteLine($"[LOGIN] Role: {user.Role}");
                Console.WriteLine($"[LOGIN] PasswordHash length: {user.PasswordHash?.Length ?? 0} chars");

                // ✅ Fix warning: Check for null PasswordHash
                if (user.PasswordHash == null)
                {
                    Console.WriteLine($"[LOGIN] Password hash is null for user: {request.Username}");
                    return null;
                }

                bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!passwordValid)
                {
                    Console.WriteLine($"[LOGIN] Invalid password for user: {request.Username}");
                    return null;
                }

                Console.WriteLine($"[LOGIN] Password verified for user: {request.Username}");

                if (user.PasswordHash.Length == 44)
                {
                    Console.WriteLine($"[LOGIN] Upgrading password from SHA256 to BCrypt for: {request.Username}");
                    user.PasswordHash = _hasher.Hash(request.Password);
                    _userRepository.Update(user);
                    await _userRepository.SaveChangesAsync();
                    Console.WriteLine($"[LOGIN] Password upgraded successfully");
                }

                Console.WriteLine($"[LOGIN] Generating JWT token for: {request.Username}");
                var token = _tokenGenerator.Generate(user);
                var expiration = DateTime.UtcNow.AddHours(8);

                Console.WriteLine($"[LOGIN] Generating refresh token for: {request.Username}");
                var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var refreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = refreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                Console.WriteLine($"[LOGIN] SUCCESS for user: {request.Username}");

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
                Console.WriteLine($"[LOGIN] CRITICAL ERROR: {ex.Message}");
                Console.WriteLine($"[LOGIN] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            try
            {
                Console.WriteLine($"[REFRESH] Attempt with token");

                var user = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken);

                if (user == null)
                {
                    Console.WriteLine($"[REFRESH] No user found with this refresh token");
                    return null;
                }

                if (user.RefreshTokenExpiry == null)
                {
                    Console.WriteLine($"[REFRESH] Refresh token expiry is null for user: {user.Username}");
                    return null;
                }

                if (user.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    Console.WriteLine($"[REFRESH] Refresh token expired for user: {user.Username}");
                    return null;
                }

                Console.WriteLine($"[REFRESH] Valid refresh token for user: {user.Username}");

                var newAccessToken = _tokenGenerator.Generate(user);
                var newExpiration = DateTime.UtcNow.AddHours(8);
                var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var newRefreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = newRefreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                Console.WriteLine($"[REFRESH] SUCCESS for user: {user.Username}");

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
                Console.WriteLine($"[REFRESH] CRITICAL ERROR: {ex.Message}");
                throw;
            }
        }

        public async Task SeedAdminAsync()
        {
            try
            {
                Console.WriteLine($"[SEED] Checking if admin exists...");

                var adminExists = await _userRepository.AnyAsync(u => u.Role == "Admin");

                if (adminExists)
                {
                    Console.WriteLine($"[SEED] Admin already exists, skipping seed");
                    return;
                }

                Console.WriteLine($"[SEED] Creating admin user...");

                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = _hasher.Hash("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(admin);
                await _userRepository.SaveChangesAsync();

                Console.WriteLine($"[SEED] Admin created successfully");
                Console.WriteLine($"   Username: admin");
                Console.WriteLine($"   Password: admin123");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEED] ERROR creating admin: {ex.Message}");
                throw;
            }
        }
    }
}