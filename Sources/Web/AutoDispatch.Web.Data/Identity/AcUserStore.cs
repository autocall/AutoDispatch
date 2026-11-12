using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Identity;

public class AcUserStore : UserStore<AcUser, AcRole, AcDbContext, int, AcUserClaim, AcUserRole, AcUserLogin, AcUserToken, AcRoleClaim> {
    public AcUserStore(AcDbContext context) : base(context) {
    }
}

public class AcRoleStore : RoleStore<AcRole, AcDbContext, int, AcUserRole, AcRoleClaim> {
    public AcRoleStore(AcDbContext context) : base(context) {
    }
}
