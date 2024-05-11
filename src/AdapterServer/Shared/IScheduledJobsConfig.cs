using Hangfire;
using Hangfire.Common;

namespace AdapterServer.Shared;

public interface IScheduledJobsConfig<Target> : IScheduledJobsConfig where Target : class
{
    /// <summary>
    /// Schedules request/response jobs as appropriate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="topic"></param>
    /// <param name="providerSessionId"></param>
    /// <param name="consumerSessionId"></param>
    /// <param name="data">Additional data used by an implementation to configure the jobs</param>
    /// <returns>The jobs that were scheduled as a dictionary of session ID -> job ID mappings</returns>
    public IDictionary<string, string> ScheduleJobs<T>(string topic, string providerSessionId, string consumerSessionId, T? data) where T : notnull;

    /// <summary>
    /// Removes the jobs configured via @ScheduleJobs.
    /// </summary>
    /// <param name="topic">The message topic to unschedule, if desired.</param>
    public void UnscheduleJobs(string? topic="");

    // TODO: is this necessary on the unschedule side of things?
    /// <summary>
    /// Removes the jobs configured via @ScheduleJobs.
    /// Provides additional context data that can be used by a specific implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="topic">The message topic to unschedule, if desired.</param>
    /// <param name="context">Optional context data</param>
    // public void UnscheduleJobs<T>(string? topic="", T? context = default) where T : notnull;
}

public interface IScheduledJobsConfig
{
    /// <summary>
    /// Helper to clean up jobs that were scheduled based on a recurring job.
    /// Does not clean up generally scheduled or enqueued jobs.
    /// </summary>
    /// <param name="recurringJobId">The ID of the recurring job</param>
    /// <param name="logger">(Optional) logger to which status messages will be logged</param>
    protected static void DeleteScheduled(string recurringJobId, ILogger? logger = null)
    {
        // Clean up any scheduled instances of the recurring job.
        // Hangfire seems to have an error in that it can have a `long` number of
        // scheduled jobs, but only takes an `int` offset for the query. This means
        // it cannot page through all scheduled jobs; however, we do not expect
        // the number of scheduled jobs to exceed the size of an `int`.
        var jobsApi = JobStorage.Current.GetMonitoringApi();
        var connection = JobStorage.Current.GetConnection();
        var scheduledCount = jobsApi.ScheduledCount();
        var pageSize = (int) Math.Min(100, scheduledCount);

        logger?.LogDebug("There are {ScheduledCount} scheduled jobs", scheduledCount);

        if (scheduledCount > int.MaxValue)
        {
            logger?.LogWarning("Number of scheduled jobs exceeds `int` value. May not be able to ensure all scheduled monitoring jobs are cancelled.");
        }

        for (int i = 0; i < Math.Min(scheduledCount, int.MaxValue); i += pageSize)
        {
            var deletedCount = jobsApi.ScheduledJobs(i, pageSize)
                .Where(pair => SerializationHelper.Deserialize<string>(connection.GetJobParameter(pair.Key, "RecurringJobId")) == recurringJobId)
                .Count(pair => {
                    if (BackgroundJob.Delete(pair.Key))
                    {
                        logger?.LogInformation("Deleted scheduled monitor job {JobId}", pair.Key);
                        return true;
                    }
                    else
                    {
                        logger?.LogWarning("Failed to delete scheduled monitor job {JobId}", pair.Key);
                        return false;
                    }
                });
            var updatedScheduledCount = jobsApi.ScheduledCount();
            i += (int)Math.Min(updatedScheduledCount - scheduledCount, -deletedCount);
            scheduledCount = updatedScheduledCount;
        }
    }
}