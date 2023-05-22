using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Test;

public class NotificationStatesTest
{
    [Fact]
    public async void NotificationStatesSimpleTest()
    {
        Notification? expectedNotification = null;
        NotificationState? expectedState = null;
        using (var context = await MockContextFactory.Factory.CreateDbContext(nameof(NotificationStatesSimpleTest)))
        {
            var notification = new Notification
            {
                Data = JsonDocument.Parse("{}"),
                ReadState = ObjectModel.ReadState.UserDependent,
                Scope = ObjectModel.Scope.Local,
                Topic = Guid.NewGuid().ToString()
            };

            // var state = new NotificationState(notification, "wheezy", ObjectModel.ReadState.Unread);
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

        var context2 = await MockContextFactory.Factory.CreateDbContext(nameof(NotificationStatesSimpleTest));
        Assert.NotNull(context2.NotificationStates.First().Notification);
        Assert.Single(context2.NotificationStates, expectedState);
        Assert.Equal(expectedNotification, context2.NotificationStates.First().Notification);
    }
}