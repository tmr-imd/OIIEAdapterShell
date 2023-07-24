
namespace AdapterServer.Pages.Request;

public interface IScheduledJobsConfig
{
    public void ScheduleJobs(RequestViewModel.MessageTypes messageType, string topic, string providerSessionid, string consumerSessionId);
    public void UnscheduleJobs();
}