using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Test;

public class NotificationStatesTest
{
    [Fact]
    public async void NotificationStatesParentNotificationAutoIncluded()
    {
        Notification? expectedNotification = null;
        NotificationState? expectedState = null;
        using (var context = await MockContextFactory.Factory.CreateDbContext(nameof(NotificationStatesParentNotificationAutoIncluded)))
        {
            var notification = new Notification
            {
                Data = JsonDocument.Parse("{}"),
                ReadState = ObjectModel.ReadState.UserDependent,
                Scope = ObjectModel.Scope.Local,
                Topic = Guid.NewGuid().ToString()
            };

            var state = new NotificationState()
            {
                Notification = notification, 
                Principal = "wheezy", 
                ReadState = ObjectModel.ReadState.Unread
            };

            context.Add(notification);
            context.Add(state);
            context.SaveChanges();

            expectedNotification = notification with { };
            expectedState = state with { Notification = expectedNotification };
        }

        var context2 = await MockContextFactory.Factory.CreateDbContext(nameof(NotificationStatesParentNotificationAutoIncluded));
        Assert.NotNull(context2.NotificationStates.Where(x => x.Principal == expectedState.Principal).First().Notification);
        Assert.Single(context2.NotificationStates, expectedState);
    }

    [Fact]
    public async void NotificationStatesNotAutoIncluded()
    {
        Notification? expected = null;
        using (var context = await MockContextFactory.Factory.CreateDbContext(nameof(NotificationStatesNotAutoIncluded)))
        {
            var notification = new Notification
            {
                Data = JsonDocument.Parse("{}"),
                ReadState = ObjectModel.ReadState.UserDependent,
                Scope = ObjectModel.Scope.Local,
                Topic = Guid.NewGuid().ToString()
            };

            var state = new NotificationState()
            {
                Notification = notification, 
                Principal = "wheezy", 
                ReadState = ObjectModel.ReadState.Unread
            };

            context.Add(notification);
            context.Add(state);
            context.SaveChanges();

            expected = notification with { };
        }

        var context2 = await MockContextFactory.Factory.CreateDbContext(nameof(NotificationStatesNotAutoIncluded));
        Assert.Empty(context2.Notifications.Where(x => x.Id == expected.Id).First().NotificationStates);
    }

    [Fact]
    public async void MultipleNotificationStatesTest()
    {
        Notification? expectedNotification = null;
        List<NotificationState> expectedStates = new List<NotificationState>();
        using (var context = await MockContextFactory.Factory.CreateDbContext(nameof(MultipleNotificationStatesTest)))
        {
            var notification = new Notification
            {
                Data = JsonDocument.Parse("{}"),
                ReadState = ObjectModel.ReadState.UserDependent,
                Scope = ObjectModel.Scope.Local,
                Topic = Guid.NewGuid().ToString()
            };
            context.Add(notification);
            context.SaveChanges();

            expectedNotification = notification with { };

            for (int i = 0; i < 10; ++i)
            {
                var state = new NotificationState()
                {
                    Notification = notification,
                    Principal = $"wheezy{i}",
                    ReadState = ObjectModel.ReadState.Unread
                };

                context.Add(state);
                context.SaveChanges();

                expectedStates.Add(state with { Notification = expectedNotification });
            }
        }

        var context2 = await MockContextFactory.Factory.CreateDbContext(nameof(MultipleNotificationStatesTest));
        Assert.Empty(context2.Notifications.Where(x => x.Id == expectedNotification.Id).First().NotificationStates);
        Assert.Equal(
            context2.Notifications
                .Include(x => x.NotificationStates)
                .Where(x => x.Id == expectedNotification.Id).First().NotificationStates, 
            expectedStates
        );
    }
}