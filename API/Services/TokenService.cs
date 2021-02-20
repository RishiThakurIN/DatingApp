using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        // SymmetricSecurityKey : Only one key will be used to sign and verify a JWT token
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            // Adding claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId,user.UserName)
            };

            // Creating some credentials with key defined in config
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // Token description i.e how token going to look
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            // Creating token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Creating token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // returning token
            return tokenHandler.WriteToken(token);
        }
    }
}
