using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Planera.Data;

public class User : IdentityUser
{
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}