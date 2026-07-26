using System.ComponentModel.DataAnnotations;
using Planera.Api.Data.Projects;

namespace Planera.Api.Data.Dto;

public class NotificationTriggerDto
{
    [Required]
    public required NotificationTriggerKind Trigger { get; set; }
    
    public string? Threshold { get; set; }
    
    public NotificationThresholdUnit? ThresholdUnit { get; set; }
    
    [Required]
    public required NotificationActionKind Action { get; set; }
}