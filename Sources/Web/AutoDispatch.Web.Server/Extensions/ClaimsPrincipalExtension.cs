using System.Security.Claims;

namespace AutoDispatch.Web.Server.Extensions;

public static class ClaimsPrincipalExtension {

    public static int GetUserId(this ClaimsPrincipal principal) {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim == null ? default : Int32.Parse(claim.Value);
    }

    public static int GetCompanyId(this ClaimsPrincipal principal) {
        var claim = principal.FindFirst("CompanyId");
        if (claim == null) {
            throw new Exception("CompanyId claim not found");
        }
        return Int32.Parse(claim.Value);
    }

    public static string GetEmail(this ClaimsPrincipal principal) {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static bool IsInRole(this ClaimsPrincipal principal, Enum role) {
        return principal.IsInRole(role.ToString());
    }
}
