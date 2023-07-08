using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace Planera.Data;

public class User : IdentityUser
{
    public override string UserName
        => base.UserName!;

    public override string Email
        => base.Email!;

    public ICollection<Project> Projects { get; init; } = new List<Project>();
}