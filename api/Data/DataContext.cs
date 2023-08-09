using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

public class DataContext : IdentityDbContext<User>
{
    public DbSet<Project> Projects { get; set; } = null!;

    public DbSet<Ticket> Tickets { get; set; } = null!;

    public DbSet<ProjectParticipant> ProjectParticipants { get; set; } = null!;

    public DbSet<TicketAssignee> TicketAssignees { get; set; } = null!;

    public DbSet<Invitation> Invitations { get; set; } = null!;

    public DataContext(DbContextOptions<DataContext> context)
        : base(context)
    {
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

}