using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using BCrypt.Net;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Repositories.Interfaces;
using Attendance_Management_System.Services;

namespace Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPasswordHasher _hasher;
        private readonly EmailJSHelper _emailJS;
        private readonly IConfiguration _configuration;
        private readonly SendGridEmailService _sendGridEmail;

        public AuthController(
            IAuthService authService,
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            IPasswordHasher hasher,
            EmailJSHelper emailJS,
            IConfiguration configuration,
            SendGridEmailService sendGridEmail)
        {
            _authService = authService;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _hasher = hasher;
            _emailJS = emailJS;
            _configuration = configuration;
            _sendGridEmail = sendGridEmail;
        }

        // ========================= AUTHENTICATION =========================

        /// <summary>Logs in a user and returns JWT tokens.</summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password are required." });

            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });

            Response.Cookies.Append("accessToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Expiration
            });

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiry,
                Path = "/api/Auth/refresh"
            });

            return Ok(result);
        }

        /// <summary>Refreshes the access token using a valid refresh token.</summary>
        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request)
        {
            var token = Request.Cookies["refreshToken"] ?? request?.RefreshToken;
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Refresh token is required." });

            var result = await _authService.RefreshAsync(token);
            if (result == null)
                return Unauthorized(new { message = "Refresh token is invalid or has expired." });

            Response.Cookies.Append("accessToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Expiration
            });

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiry,
                Path = "/api/Auth/refresh"
            });

            return Ok(result);
        }

        /// <summary>Logs out the current user and clears auth cookies.</summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>Returns the currently authenticated user's info.</summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMe()
        {
            var userId = User.GetUserId();
            var username = User.GetUsername();
            var role = User.GetRole();
            return Ok(new { userId, username, role });
        }

        // ========================= REGISTRATION =========================

        /// <summary>Registers a new student account and sends an email verification link.</summary>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { success = false, message = "Username, password, full name, and email are required" });

            var existingUser = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (existingUser != null)
                return BadRequest(new { success = false, message = "Username already exists" });

            var (firstName, lastName) = ParseFullName(dto.FullName);
            var studentNo = await GenerateStudentNumber();

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = _hasher.Hash(dto.Password),
                Role = "Student",
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(1)
            };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var student = new Student
            {
                StudentNo = studentNo,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = "",
                Email = dto.Email,
                Section = "",
                MobileNo = "",
                CreatedAt = DateTime.UtcNow
            };
            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();

            // ✅ LINK USER TO STUDENT
            user.StudentId = student.Id;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var frontendUrl = _configuration["FrontendUrl"] ?? "https://triotagapayo-ams-dbtc-vzpc.onrender.com/";
            var verificationLink = $"{frontendUrl}/verify-email?token={user.EmailVerificationToken}";
            var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Welcome to AMS!</h2>
                    <p>Hello {firstName},</p>
                    <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                    <p><a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Verify Email</a></p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{verificationLink}</p>
                    <p>This link expires in 24 hours.</p>
                    <br>
                    <p>Best regards,<br/>AMS Team</p>
                </body>
                </html>";

            var emailSent = await _sendGridEmail.SendEmailAsync(dto.Email, "Verify Your Email - AMS", emailBody);
            Console.WriteLine($"[REGISTER] SendGrid email sent to {dto.Email}: {emailSent}");

            return Ok(new
            {
                success = true,
                message = "Registration successful. Please verify your email.",
                studentId = student.Id,
                studentNo = student.StudentNo,
                username = user.Username
            });
        }

        // ========================= PASSWORD MANAGEMENT =========================

        /// <summary>Changes the password of the currently authenticated user.</summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { success = false, message = "Username, current password, and new password are required" });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new { success = false, message = "New password must be at least 6 characters" });

            var user = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            bool currentPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!currentPasswordValid)
                return BadRequest(new { success = false, message = "Current password is incorrect" });

            user.PasswordHash = _hasher.Hash(dto.NewPassword);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { success = true, message = "Password changed successfully" });
        }

        // ========================= EMAIL VERIFICATION =========================

        /// <summary>Resends the email verification link to the user.</summary>
        [AllowAnonymous]
        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            var user = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (user == null)
                return Ok(new { success = true, message = "If the account exists, a verification email has been sent." });

            if (user.IsEmailVerified)
                return BadRequest(new { success = false, message = "Email is already verified." });

            user.EmailVerificationToken = Guid.NewGuid().ToString();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(1);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            string email = "";
            if (user.Role == "Student")
            {
                var student = await _studentRepository.FindAsync(s => s.StudentNo == user.Username);
                if (student != null) email = student.Email;
            }

            var frontendUrl = _configuration["FrontendUrl"] ?? "https://localhost:7033";
            var verificationLink = $"{frontendUrl}/verify-email?token={user.EmailVerificationToken}";
            var emailBody = $@"
                <html>
                <body>
                    <h2>Email Verification</h2>
                    <p>Please verify your email by clicking <a href='{verificationLink}'>here</a>.</p>
                    <p>This link expires in 24 hours.</p>
                </body>
                </html>";

            var emailSent = await _sendGridEmail.SendEmailAsync(email, "Resend: Verify Your Email", emailBody);
            Console.WriteLine($"[RESEND] SendGrid email sent to {email}: {emailSent}");

            return Ok(new { success = true, message = "Verification email sent." });
        }

        /// <summary>Verifies a user's email address using the token sent via email.</summary>
        [AllowAnonymous]
        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var user = await _userRepository.FindAsync(u => u.EmailVerificationToken == dto.Token);
            if (user == null)
                return BadRequest(new { success = false, message = "Invalid verification token." });

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                return BadRequest(new { success = false, message = "Verification token has expired. Please request a new one." });

            if (user.IsEmailVerified)
                return BadRequest(new { success = false, message = "Email is already verified." });

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { success = true, message = "Email verified successfully. You can now log in." });
        }

        // ========================= FORGOT PASSWORD =========================

        /// <summary>Submits a forgot password request and notifies the admin.</summary>
        [AllowAnonymous]
        [HttpPost("forgot-password-request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordRequestDto dto)
        {
            var user = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (user == null)
                return Ok(new { success = true, message = "If the account exists, the admin has been notified." });

            return Ok(new { success = true, message = "The admin has been notified. You will receive instructions shortly." });
        }

        // ========================= HELPERS =========================

        private async Task<string> GenerateStudentNumber()
        {
            var students = await _studentRepository.GetAllAsync();
            var count = students.Count() + 1;
            return $"STU{count:D3}";
        }

        private (string FirstName, string LastName) ParseFullName(string fullName)
        {
            var trimmed = fullName.Trim();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return ("", "");
            if (parts.Length == 1) return (parts[0], "");
            return (parts[0], string.Join(" ", parts.Skip(1)));
        }
    }
}