using System.Text.Json;

namespace Notifications.Test;

public class NotificationsContextTest
{
    [Fact]
    public async void TestDbInit()
    {
        NotificationsContext context = await MockContextFactory.Factory.CreateDbContext("test");
        context.Notifications.ToList();
    }

    [Fact]
    public async void NotificationJsonDocumentConversionTest()
    {
        var context = await MockContextFactory.Factory.CreateDbContext("test");

        var notification = new Notification();
        Assert.Null(notification.Data);

        context.Add(notification);
        Assert.Throws<Microsoft.EntityFrameworkCore.DbUpdateException>(
            () => context.SaveChanges()
        );

        notification.Data = JsonDocument.Parse(@"{""test"": ""value""}");
        context.SaveChanges();

        var context2 = await MockContextFactory.Factory.CreateDbContext("test2");
        var result = context.Notifications.Where(x => x.Id == notification.Id).First();
        Assert.Equal(notification, result);
        var property = result.Data.RootElement.EnumerateObject().First();
        Assert.Equal("test", property.Name);
        Assert.Equal("value", property.Value.GetString());
    }
}