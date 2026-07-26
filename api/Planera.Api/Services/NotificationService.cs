using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Planera.Api.Data;
using Planera.Api.Data.Notifications;
using Planera.Api.Models.Notifications;

namespace Planera.Api.Services;

public class NotificationService(DataContext dataContext)
{
    private readonly DataContext _dataContext = dataContext;

    public async Task<ErrorOr<Created>> SubscribeAsync(string userId, string endpoint, NotificationSubscriptionKeys keys)
    {
        var subscription = new PushNotificationSubscription
        {
            UserId = userId,
            Endpoint = endpoint,
            P256Dh = keys.P256Dh,
            Auth = keys.Auth,
        };

        await _dataContext.PushNotificationSubscriptions.AddAsync(subscription);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Created>();
    }

    public async Task<ErrorOr<Created>> RefreshAsync(string userId, string oldEndpoint, string newEndpoint, NotificationSubscriptionKeys keys)
    {
        var subscription = await _dataContext
            .PushNotificationSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Endpoint == oldEndpoint);
        if (subscription == null)
            return await SubscribeAsync(userId, newEndpoint, keys);
        
        subscription.Endpoint = newEndpoint;
        subscription.P256Dh = keys.P256Dh;
        subscription.Auth = keys.Auth;

        _dataContext.PushNotificationSubscriptions.Update(subscription);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Created>();
    }

    public async Task<ErrorOr<Deleted>> UnsubscribeAsync(PushNotificationSubscription subscription)
    {
        try
        {
            _dataContext.Remove(subscription);
            await _dataContext.SaveChangesAsync();

            return new ErrorOr<Deleted>();
        }
        catch
        {
            return Error.Unexpected();
        }
    }

    public async Task<ErrorOr<List<PushNotificationSubscription>>> GetSubscriptionsForUserAsync(string userId)
    {
        return await _dataContext
            .PushNotificationSubscriptions
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<ErrorOr<List<PushNotificationSubscription>>> GetSubscriptionsForUsersInProjectAsync(string projectId)
    {
        return await _dataContext
            .PushNotificationSubscriptions
            .Where(x => x.User.JoinedProjects.Any(p => p.Id == projectId))
            .ToListAsync();
    }
}