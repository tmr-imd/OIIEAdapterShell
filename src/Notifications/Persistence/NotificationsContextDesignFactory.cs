using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Security.Claims;

namespace Notifications.Persistence;

public class JobContextDesignFactory : IDesignTimeDbContextFactory<NotificationsContext>
{
    private const string connectionString = "Server=.\\sqlexpress;Database=notifications;Trusted_Connection=True;MultipleActiveResultSets=True";

    public NotificationsContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<NotificationsContext>();

        return new NotificationsContext(optionsBuilder.Options, "migrations");
    }
}
