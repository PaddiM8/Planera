using Microsoft.EntityFrameworkCore;
using Planera.Api.Data;
using Planera.Api.Data.Notifications;
using Planera.Api.Data.Projects;
using Planera.Api.Data.Tickets;
using Planera.Api.Utility;

namespace Planera.Api.Services;

public class NotificationScheduler(DataContext dataContext)
{
    private readonly DataContext _dataContext = dataContext;

    public async Task SetNotificationTriggersForProjectAsync(ICollection<NotificationTrigger> existingTriggerRules, ICollection<NotificationTrigger> newTriggerRules)
    {
        var strategy = _dataContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Find removed rules and remove the rules and the queue entries from the database
            var removedRules = existingTriggerRules.ExceptBy(newTriggerRules.Select(t => t.Id), t => t.Id);
            var removedIds = removedRules.Select(x => x.Id).ToList();
            await _dataContext
                .NotificationTriggers
                .Where(x => removedIds.Contains(x.Id))
                .ExecuteDeleteAsync();
            await _dataContext
                .NotificationQueue
                .Where(x => removedIds.Contains(x.NotificationTriggerId))
                .ExecuteDeleteAsync();

            // Find updated rules and update the queue entries
            var ruleComparer = new NotificationTriggerComparer();
            foreach (var newRule in newTriggerRules)
            {
                var oldRule = existingTriggerRules.FirstOrDefault(x => x.Id == newRule.Id);
                if (oldRule == null)
                {
                    // Added
                    _dataContext.NotificationTriggers.Add(newRule);

                    if (newRule.Trigger == NotificationTriggerKind.TimeUntilDeadline)
                    {
                        await AddRuleForTimeUntilDeadlineAsync(newRule);
                    }
                }
                else if (!ruleComparer.Equals(newRule, oldRule))
                {
                    // Updated
                    _dataContext.NotificationTriggers.Update(newRule);
                    
                    if (newRule.Trigger == NotificationTriggerKind.TimeUntilDeadline)
                    {
                        await UpdateRuleForTimeUntilDeadlineAsync(oldRule, newRule);
                    }
                }
            }
            
            await _dataContext.SaveChangesAsync();
        });
    }
    
    private async Task AddRuleForTimeUntilDeadlineAsync(NotificationTrigger rule)
    {
        var tickets = _dataContext
            .Tickets
            .Include(t => t.Author)
            .Include(t => t.Project)
            .Where(t => t.ProjectId == rule.ProjectId)
            .Where(t => t.Status == TicketStatus.Inactive || t.Status == TicketStatus.None)
            .Where(t => t.Deadline != null);
        foreach (var ticket in tickets)
            await HandleTimeUntilDeadlineAsync(ticket, ticket.Author.UserName!, ticket.Project, rule, isNew: true, shouldSave: false);
    }

    private async Task UpdateRuleForTimeUntilDeadlineAsync(NotificationTrigger oldRule, NotificationTrigger newRule)
    {
        var newTimeSpan = newRule.ResolveThresholdAsTimeSpan();
        if (!newTimeSpan.HasValue)
            return;

        // Adjust the scheduled time to match with the new threshold
        var timeToAdd = newTimeSpan.Value - oldRule.ResolveThresholdAsTimeSpan()!.Value;
        await _dataContext
            .NotificationQueue
            .Where(t => t.NotificationTriggerId == newRule.Id)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(e => e.ScheduledTime, p => p.ScheduledTime + timeToAdd)
            );

        // Remove any entries that got pushed back to the past to prevent notification spam
        var roughlyNow = DateTime.UtcNow.AddHours(-1);
        await _dataContext
            .NotificationQueue
            .Where(t => t.NotificationTriggerId == newRule.Id)
            .Where(t => t.ScheduledTime < roughlyNow)
            .ExecuteDeleteAsync();
    }
    
    public async Task ScheduleAsync(NotificationQueueEntry entry, bool isNew, bool shouldSave = true)
    {
        if (!isNew)
        {
            var existing = await _dataContext.NotificationQueue.FindAsync(
                entry.TargetId,
                entry.TargetKind,
                entry.NotificationTriggerId,
                entry.ObjectId
            );
            if (existing != null)
            {
                _dataContext.NotificationQueue.Update(existing);
                if (shouldSave)
                    await _dataContext.SaveChangesAsync();
                
                return;
            }
        }

        _dataContext.NotificationQueue.Add(entry);
        if (shouldSave)
            await _dataContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(string targetId, NotificationTargetKind targetKind, int notificationTriggerId, string objectId)
    {
        var entry = await _dataContext.NotificationQueue.FindAsync(targetId, targetKind, notificationTriggerId, objectId);
        if (entry != null)
        {
            _dataContext.NotificationQueue.Remove(entry);
            await _dataContext.SaveChangesAsync();
        }
    }
    
    public async Task ScheduleForTicketAsync(
        Ticket ticket,
        string authorName, 
        Project project,
        IEnumerable<NotificationTrigger> triggerRules,
        bool isNew
    )
    {
        if (ticket.Deadline == null)
            return;

        foreach (var triggerRule in triggerRules)
        {
            if (triggerRule.Trigger == NotificationTriggerKind.TimeUntilDeadline)
            {
                await HandleTimeUntilDeadlineAsync(ticket, authorName, project, triggerRule, isNew);
            }
        }
    }

    private async Task HandleTimeUntilDeadlineAsync(
        Ticket ticket,
        string authorName,
        Project project,
        NotificationTrigger triggerRule,
        bool isNew,
        bool shouldSave = true
    )
    {
        if (ticket.Status is not (TicketStatus.None or TicketStatus.Inactive))
        {
            if (!isNew)
                await RemoveAsync(ticket.ProjectId, NotificationTargetKind.Project, triggerRule.Id, ticket.Id.ToString());

            return;
        }

        var threshold = triggerRule.ResolveThresholdAsTimeSpan();
        if (!threshold.HasValue || !ticket.Deadline.HasValue)
            return;

        var assignedUserIds = ticket
            .Assignees
            .Select(u => u.Id)
            .ToList();
        if (assignedUserIds.Count == 0)
            assignedUserIds.Add(ticket.AuthorId);

        var scheduledTime = ticket.Deadline.Value - threshold.Value;
        var entry = new NotificationQueueEntry
        {
            TargetId = ticket.ProjectId,
            TargetKind = NotificationTargetKind.Project,
            NotificationTriggerId = triggerRule.Id,
            ObjectId = ticket.Id.ToString(),
            ScheduledTime = scheduledTime,
            AssignedUserIds = assignedUserIds,
            TriggerKind = triggerRule.Trigger,
            ActionKind = triggerRule.Action,
            NotificationKind = NotificationKinds.DeadlineMyTicket,
            Title = $"({project.Name}) Approaching deadline for #{ticket.Id} {ticket.Title.Truncate(25)}",
            Content = $"Due for {triggerRule.Threshold} {triggerRule.ThresholdUnit.ToString()?.ToLower()}: '{ticket.Title.Truncate(50)}'".Truncate(500),
            Url = $"/projects/{authorName}/{project.Slug}/{ticket.Id}",
        };

        await ScheduleAsync(entry, isNew, shouldSave);
    }
}