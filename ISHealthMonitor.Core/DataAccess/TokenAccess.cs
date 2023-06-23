using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ISHealthMonitor.Core.DataAccess
{
    public class TokenService
    {
        private readonly string _secret;
        private readonly string _expDate;
        private readonly string _audience;
        private readonly string _issuer;
        public TokenService(IConfiguration config)
        {
            _audience = config.GetSection("TokenConfig").GetSection("audience").Value;
            _issuer = config.GetSection("TokenConfig").GetSection("issuer").Value;
            _secret = config.GetSection("TokenConfig").GetSection("secret").Value;
            _expDate = config.GetSection("TokenConfig").GetSection("expirationInMinutes").Value;
        }

        public string GenerateSecurityToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userName)
                }),
                Issuer = _issuer,
                Audience = _audience,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_expDate)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}
