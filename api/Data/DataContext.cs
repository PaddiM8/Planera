using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

public class DataContext : IdentityDbContext<User>
{
    public DbSet<Project> Projects { get; set; } = null!;

    public DataContext(DbContextOptions<DataContext> context)
        : base(context)
    {
    }
}