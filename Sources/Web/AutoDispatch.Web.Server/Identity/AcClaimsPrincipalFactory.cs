using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Server.Identity;

/// <summary>
///     Claims cookie-входа: к стандартным Identity добавляются CompanyId и TimeZoneId (как в JWT из AuthService) </summary>
public class AcClaimsPrincipalFactory : UserClaimsPrincipalFactory<AcUser, AcRole> {

    public AcClaimsPrincipalFactory(UserManager<AcUser> userManager, RoleManager<AcRole> roleManager, IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AcUser user) {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
        identity.AddClaim(new Claim("TimeZoneId", user.TimeZoneId ?? "UTC"));
        return identity;
    }
}
