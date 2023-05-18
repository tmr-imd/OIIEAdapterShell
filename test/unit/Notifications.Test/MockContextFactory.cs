using Microsoft.Extensions.Configuration;
using System.Text;

namespace Notifications.Test;

public class MockContextFactory : NotificationsContextFactory
{
    public static NotificationsContextFactory Factory
    {
        get;
        private set;
    }

    static MockContextFactory() {
        var defaults = @"{
            ""ConnectionStrings"": {
                ""DefaultConnection"": ""db\\notifications.db"",
                ""DefaultConnection_Provider"": ""Microsoft.Data.Sqlite""
            }
        }";

        var config = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(defaults)))
            .Build();

        Factory = new MockContextFactory(config);

        var context = Factory.CreateDbContext("");
        context.Wait();
        context.Result.Database.EnsureDeleted();
        context.Result.Database.EnsureCreated();
    }

    private MockContextFactory(IConfiguration config) : base(config)
    {
    }
}