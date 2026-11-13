using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Identity;

public class AdUserStore : UserStore<AdUser, AdRole, AdDbContext, Guid, AdUserClaim, AdUserRole, AdUserLogin, AdUserToken, AdRoleClaim> {
    public AdUserStore(AdDbContext context) : base(context) {
    }
}

public class AdRoleStore : RoleStore<AdRole, AdDbContext, Guid, AdUserRole, AdRoleClaim> {
    public AdRoleStore(AdDbContext context) : base(context) {
    }
}
