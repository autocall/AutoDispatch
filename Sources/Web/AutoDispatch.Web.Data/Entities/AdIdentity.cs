using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

public class AdRole : IdentityRole<Guid> {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public override Guid Id { get; set; }
}

public class AdUserRole : IdentityUserRole<Guid> { }
public class AdUserClaim : IdentityUserClaim<Guid> { }
public class AdRoleClaim : IdentityRoleClaim<Guid> { }
public class AdUserLogin : IdentityUserLogin<Guid> { }
public class AdUserToken : IdentityUserToken<Guid> { }
