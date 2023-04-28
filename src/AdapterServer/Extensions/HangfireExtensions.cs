using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage.SQLite;

namespace AdapterServer.Extensions;

public static class HangfireExtensions
{
    public static Action<IGlobalConfiguration> HangfireConfiguration(this IServiceCollection services, string? connectionString, string? storage)
    {
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("connectionString cannot be null or empty", nameof(connectionString));
        if (string.IsNullOrEmpty(storage)) throw new ArgumentException("storage cannot be null or empty", nameof(storage));

        return storage switch
        {
            "SQLiteStorage" => configuration => AddHangfireSqlite(configuration, connectionString),
            "SqlServerStorage" => configuration => AddHangfireSqlServer(configuration, connectionString),
            _ => throw new Exception($"Unknown configuration option for Hangfire storage: ${storage}")
        };
    }

    private static IGlobalConfiguration AddHangfireSqlite(IGlobalConfiguration configuration, string connectionString)
    {
        return configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(connectionString, new SQLiteStorageOptions
            {
                InvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(5),
                DistributedLockLifetime = TimeSpan.FromSeconds(30),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5)
            });
    }

    private static IGlobalConfiguration AddHangfireSqlServer(IGlobalConfiguration configuration, string connectionString)
    {
        return configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
    }
}
