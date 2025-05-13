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

        public JwtHelper(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.RoleId.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) 
    };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
