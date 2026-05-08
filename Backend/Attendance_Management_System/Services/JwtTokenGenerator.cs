using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.DBCONTEXT;
using Microsoft.EntityFrameworkCore;

namespace Attendance_Management_System.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public JwtTokenGenerator(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string Generate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // ADD TEACHERID CLAIM IF USER IS TEACHER
            if (user.Role == "Teacher")
            {
                try
                {
                    // Try to find teacher by username
                    var teacher = _context.Teachers
                        .FirstOrDefault(t => t.TeacherNo == user.Username || t.Email == user.Username);

                    if (teacher != null)
                    {
                        claims.Add(new Claim("TeacherId", teacher.Id.ToString()));
                        claims.Add(new Claim("TeacherNo", teacher.TeacherNo));
                    }
                }
                catch
                {
                    // Silently fail
                }
            }

            // Get JWT key with null check
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}