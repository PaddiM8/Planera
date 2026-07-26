using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planera.Api.Data.Projects;

public class NotificationTrigger
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(64)]
    public required string ProjectId { get; set; }
    
    public required NotificationTriggerKind Trigger { get; set; }
    
    [StringLength(32)]
    public string? Threshold { get; set; }
    
    public NotificationThresholdUnit? ThresholdUnit { get; set; }
    
    public required NotificationActionKind Action { get; set; }

    public TimeSpan? ResolveThresholdAsTimeSpan()
    {
        if (!int.TryParse(Threshold, out var thresholdNumber))
            return null;

        return ThresholdUnit switch
        {
            NotificationThresholdUnit.Minutes => TimeSpan.FromMinutes(thresholdNumber),
            NotificationThresholdUnit.Hours => TimeSpan.FromHours(thresholdNumber),
            NotificationThresholdUnit.Days => TimeSpan.FromDays(thresholdNumber),
            _ => null,
        };
    }
}

public class NotificationTriggerComparer : IEqualityComparer<NotificationTrigger>
{
    public bool Equals(NotificationTrigger? x, NotificationTrigger? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;
        
        return x.Id == y.Id &&
            x.ProjectId == y.ProjectId &&
            x.Trigger == y.Trigger &&
            x.Threshold == y.Threshold &&
            x.ThresholdUnit == y.ThresholdUnit &&
            x.Action == y.Action;
    }

    public int GetHashCode(NotificationTrigger obj)
    {
        return HashCode.Combine(
            obj.Id,
            obj.ProjectId,
            (int)obj.Trigger,
            obj.Threshold,
            obj.ThresholdUnit,
            (int)obj.Action
        );
    }
}