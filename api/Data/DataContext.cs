using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

public class DataContext : IdentityDbContext<User>
{
    private readonly IConfiguration _configuration;
    public DbSet<Project> Projects { get; set; } = null!;


    public DbSet<TicketAssignee> TicketAssignees { get; set; } = null!;

    public DbSet<ProjectParticipant> ProjectParticipants { get; set; } = null!;

    public DbSet<Invitation> Invitations { get; set; } = null!;

    public DbSet<Ticket> Tickets { get; set; } = null!;

    public DbSet<Note> Notes { get; set; } = null!;

    public DataContext(DbContextOptions<DataContext> context, IConfiguration configuration)
        : base(context)
    {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(e => e.Tickets)
            .WithOne(e => e.Author)
            .HasForeignKey(e => e.AuthorId)
            .IsRequired();
        modelBuilder.Entity<Ticket>()
            .HasMany(e => e.Assignees)
            .WithMany(e => e.AssignedTickets)
            .UsingEntity<TicketAssignee>();
        modelBuilder.Entity<Ticket>()
            .HasMany(e => e.Notes)
            .WithOne(e => e.Ticket)
            .HasForeignKey(e => new { e.TicketId, e.ProjectId })
            .IsRequired();
        modelBuilder.Entity<Project>()
            .HasMany(e => e.Participants)
            .WithMany(e => e.JoinedProjects)
            .UsingEntity<ProjectParticipant>();
        modelBuilder.Entity<User>()
            .HasMany(e => e.Projects)
            .WithOne(e => e.Author)
            .HasForeignKey(e => e.AuthorId);
        modelBuilder.Entity<User>()
            .HasMany(e => e.Invitations)
            .WithMany(e => e.InvitedUsers)
            .UsingEntity<Invitation>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(
            $"Host={_configuration["Postgres:Host"]};" +
                $"Username={_configuration["Postgres:User"]};" +
                $"Password={_configuration["Postgres:Password"]};" +
                $"Database={_configuration["Postgres:Database"]}"
        );
}