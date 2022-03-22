using Identity.Models;
using Identity.Setup;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Services
{
    public class TokenService
    {
        private readonly AppConfig _appSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(
            IOptions<AppConfig> appSettings,
            UserManager<ApplicationUser> userManager
        )
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenJwtAsync(ApplicationUser user)
        {
            var identityClaims = new ClaimsIdentity();

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                identityClaims.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            identityClaims.AddClaim(new Claim("IsActive", user.IsActive.ToString()));
            identityClaims.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, user.Email));
            identityClaims.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, user.Id));

            var claims = await _userManager.GetClaimsAsync(user);

            identityClaims.AddClaims(await _userManager.GetClaimsAsync(user));

            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiresHours),
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.ValidIn,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }

}
