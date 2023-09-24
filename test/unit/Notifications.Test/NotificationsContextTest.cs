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

    [Fact]
    public async void NotificationsAuditFieldsTest()
    {
        var context = await MockContextFactory.Factory.CreateDbContext("test");

        var notification = new Notification
        {
            Data = JsonDocument.Parse("{}")
        };
        Assert.Empty(notification.CreatedBy);
        Assert.Equal(default(DateTime), notification.DateCreated);
        Assert.Empty(notification.ModifiedBy);
        Assert.Equal(default(DateTime), notification.DateModified);

        context.Add(notification);
        context.SaveChanges();

        Assert.Equal("test", notification.CreatedBy);
        Assert.NotEqual(default(DateTime), notification.DateCreated);
        Assert.Equal("test", notification.ModifiedBy);
        Assert.NotEqual(default(DateTime), notification.DateModified);

        var dateCreated = notification.DateCreated;
        await Task.Delay(100); // to ensure times are different
        
        var context2 = await MockContextFactory.Factory.CreateDbContext("test2");
        var result = context.Notifications.Where(x => x.Id == notification.Id).First();
        Assert.Equal(notification, result);
        Assert.Same(notification, result); // It is actually the same across context objects

        context2.Attach(result);
        result.Topic = "changed";
        context2.SaveChanges(true);

        Assert.Equal("test", result.CreatedBy);
        Assert.Equal(dateCreated, result.DateCreated);
        Assert.Equal("test2", result.ModifiedBy);
        Assert.NotEqual(result.DateCreated, result.DateModified);
    }
}