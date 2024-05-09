
namespace AdapterServer.Shared;

public interface IScheduledJobsConfig<Target> where Target : class
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

    // TODO: is this necessary on the unschdule side of things?
    /// <summary>
    /// Removes the jobs configured via @ScheduleJobs.
    /// Provides additional context data that can be used by a specific implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="topic">The message topic to unschedule, if desired.</param>
    /// <param name="context">Optional context data</param>
    // public void UnscheduleJobs<T>(string? topic="", T? context = default) where T : notnull;
}