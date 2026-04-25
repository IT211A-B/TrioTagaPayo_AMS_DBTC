using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string Generate(User user)
        {
            // ✅ PART 1 — Claims Info
            // Kini ang "valid string" / payload sulod sa JWT
            // Ma-decode ni sa jwt.io — but dili ma-tamper (signed)
            var claims = new[]
            {
                // Who is this user? (identity claims)
                new Claim(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier,    user.Id.ToString()),
                new Claim(ClaimTypes.Name,              user.Username),
                new Claim(ClaimTypes.Role,              user.Role),

                // Token metadata claims
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()), // unique token ID
                new Claim(JwtRegisteredClaimNames.Iat,                              // issued at
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            // ✅ Signing key — must match Program.cs config
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ✅ Build the token
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