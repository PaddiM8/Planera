using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Planera.Api.Data.Projects;

[PrimaryKey(nameof(TargetId), nameof(TargetKind), nameof(NotificationTriggerId), nameof(ObjectId))]
[Index(nameof(NotificationTriggerId))]
[Index(nameof(ScheduledTime))]
public class NotificationQueueEntry
{
    [StringLength(64)]
    public required string TargetId { get; set; }
    
    public required int NotificationTriggerId { get; set; }
    
    [StringLength(64)]
    public required string ObjectId { get; set; }

    public required NotificationTargetKind TargetKind { get; set; }
    public required DateTime ScheduledTime { get; set; }
    
    public required NotificationTriggerKind TriggerKind { get; set; }
    
    public required NotificationActionKind ActionKind { get; set; }
    
    [StringLength(128)]
    public required string Title { get; set; }
    
    [StringLength(500)]
    public required string Content { get; set; }

    public string? Url { get; set; }
}