using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Planera.Api.Data.Users;

namespace Planera.Api.Data.Notifications;

[Index(nameof(UserId))]
public class PushNotificationSubscription
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [StringLength(64)]
    public required string UserId { get; set; }

    public User User { get; set; } = null!;
    
    public required string Endpoint { get; set; }
    
    [StringLength(255)]
    public required string P256Dh { get; set; }
    
    [StringLength(255)]
    public required string Auth { get; set; }
}