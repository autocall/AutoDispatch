using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     JWT для App (схема Bearer); сайт входит по cookie через SignInManager </summary>
public class AuthService : BaseService {

    public AuthService(ServicesHost host) : base(host) { }

    public async Task<string> CreateTokenAsync(AcUser user) {
        var handler = new JwtSecurityTokenHandler();
        var privateKey = Encoding.UTF8.GetBytes(AppSettings.JwtPrivateKey);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor {
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.Add(AppSettings.JwtLifetime),
            Subject = await GenerateClaimsAsync(user)
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    #region [ private ]

    private async Task<ClaimsIdentity> GenerateClaimsAsync(AcUser user) {
        var ci = new ClaimsIdentity();

        ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        ci.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        ci.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        ci.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
        ci.AddClaim(new Claim("TimeZoneId", user.TimeZoneId ?? "UTC"));

        var roles = await Host.UserManager.GetRolesAsync(user);
        foreach (var role in roles) {
            ci.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return ci;
    }

    #endregion [ private ]
}
