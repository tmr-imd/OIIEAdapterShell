using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Notifications.Persistence;

public class NotificationsContextFactory
{
    private readonly IConfiguration _config;

    public NotificationsContextFactory(IConfiguration config)
    {
        _config = config;
    }

    public Task<NotificationsContext> CreateDbContext(ClaimsPrincipal claimsPrincipal)
    {
        return CreateDbContext(claimsPrincipal.Identity?.Name ?? "");
    }

    public async Task<NotificationsContext> CreateDbContext(string who)
    {
        var builder = new DbContextOptionsBuilder<NotificationsContext>();
        var defaultConnection = _config.GetConnectionString("NotificationsConnection") ?? "notifications.db";
        builder.UseSqlite($"Filename={defaultConnection}");
        var context = new NotificationsContext(builder.Options, who);

        // await context.Database.EnsureCreatedAsync();
        await context.Database.MigrateAsync();

        return context;
    }
}
