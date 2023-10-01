
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
    public void ScheduleJobs<T>(string topic, string providerSessionId, string consumerSessionId, T? data) where T : notnull;

    /// <summary>
    /// Removes the jobs configured via @ScheduleJobs.
    /// </summary>
    public void UnscheduleJobs(string? topic="");
}