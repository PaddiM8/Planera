using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Planera.Api.Data.Notifications;

namespace Planera.Api.Data.Projects;

[PrimaryKey(nameof(TargetId), nameof(TargetKind), nameof(NotificationTriggerId), nameof(ObjectId))]
[Index(nameof(NotificationTriggerId))]
[Index(nameof(ScheduledTime))]
public class NotificationQueueEntry
{
    [StringLength(64)]
    public required string TargetId { get; set; }

    public required NotificationTargetKind TargetKind { get; set; }
    
    public required int NotificationTriggerId { get; set; }
    
    [StringLength(64)]
    public required string ObjectId { get; set; }
    
    /// <summary>
    /// List of user IDs for *primary* users to receive the notification, eg. ticket assignees.
    /// Users that have eg. enabled all deadline notifications in a project would receive
    /// the notification without being in this list.
    /// </summary>
    public required List<string>? AssignedUserIds { get; set; }

    public required DateTime ScheduledTime { get; set; }
    
    public required NotificationTriggerKind TriggerKind { get; set; }
    
    public required NotificationActionKind ActionKind { get; set; }
    
    public required NotificationKinds NotificationKind { get; set; }
    
    [StringLength(128)]
    public required string Title { get; set; }
    
    [StringLength(500)]
    public required string Content { get; set; }

    public string? Url { get; set; }
}