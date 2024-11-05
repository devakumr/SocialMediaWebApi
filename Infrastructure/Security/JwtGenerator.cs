using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryInMinutes;

        public JwtGenerator(IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            _secret = jwtSettings["Secret"];
            _issuer = jwtSettings["Issuer"];
            _audience = jwtSettings["Audience"];
            _expiryInMinutes = Convert.ToInt32(jwtSettings["TokenExpiryInMinutes"]);
        }

        public string CreateToken(string userId, IEnumerable<Claim> claims)
        {
            // Create the token's expiration time
            var expiry = DateTime.UtcNow.AddMinutes(_expiryInMinutes);

            // Create the signing credentials using the secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            // Return the serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateToken(string userId) // Change parameter name for clarity
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId), // Use NameIdentifier for user ID
                // You can add more claims if needed
            };

            return CreateToken(userId, claims); // Pass actual userId to CreateToken
        }
    }
}
