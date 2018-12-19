using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Test
{
  public class TokenService : ITokenService
  {
    AppSettings Settings = null;

    public TokenService(IOptions<AppSettings> settings = null) => this.Settings = settings.Value;

    public string BuildToken(Guid userId)
    {
      string token = string.Empty;

      // authentication successful so generate jwt token
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = System.Text.Encoding.ASCII.GetBytes(this.Settings.JWTSecretKeys[Constants.Settings.JWTSecretKeys.Site]);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.Encode()) }),
        Expires = DateTime.UtcNow.AddMonths(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };

      token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

      return token;
    }
  }
}
