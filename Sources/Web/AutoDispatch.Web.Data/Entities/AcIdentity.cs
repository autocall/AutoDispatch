using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

public class AcRole : IdentityRole<int> {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public override int Id { get; set; }
}

public class AcUserRole : IdentityUserRole<int> { }
public class AcUserClaim : IdentityUserClaim<int> { }
public class AcRoleClaim : IdentityRoleClaim<int> { }
public class AcUserLogin : IdentityUserLogin<int> { }
public class AcUserToken : IdentityUserToken<int> { }
