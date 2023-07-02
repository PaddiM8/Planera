using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace Planera.Data;

public class User : IdentityUser
{
    public override string UserName
        => base.UserName!;

    public override string Email
        => base.Email!;

    [JsonIgnore]
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}