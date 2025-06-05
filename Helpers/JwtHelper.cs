// Helpers/JwtHelper.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KDomBackend.Models.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KDomBackend.Helpers
{
    public class JwtHelper
    {
        private readonly JwtSettings _settings;

        public JwtHelper(JwtSettings settings)
        {
            _settings = settings;
            Console.WriteLine($"[DEBUG] JwtHelper - Loaded SecretKey: {_settings.SecretKey}");
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // IMPORTANT: Use the actual role name, not the RoleId
            var roleName = user.Role ?? "user"; // Use the Role property directly

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName), // Use role name instead of RoleId
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("username", user.Username), // Add explicit username claim
                new Claim("role", roleName) // Add explicit role claim for frontend
            };

            Console.WriteLine($"[DEBUG] JwtHelper - Generated claims for user {user.Username}:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine($"[DEBUG] JwtHelper - Generated token: {tokenString}");

            return tokenString;
        }
    }
}