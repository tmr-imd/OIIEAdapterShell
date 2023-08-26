using TaskQueueing.Jobs;
using TaskQueueing.Data;
using AdapterServer.Data;
using AdapterServer.Shared;
using System.Xml.Linq;
using Hangfire;

namespace AdapterServer.Pages.Request;

using RequestJobJSON = RequestProviderJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using ResponseJobJSON = RequestConsumerJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using RequestJobBOD = RequestProviderJob<ProcessGetShowStructuresJob, XDocument, XDocument>;
using ResponseJobBOD = RequestConsumerJob<ProcessGetShowStructuresJob, XDocument, XDocument>;
using MessageTypes = RequestViewModel.MessageTypes;

public class JobSchedulerForStructures : IScheduledJobsConfig<ManageRequestViewModel>
{
    public const string CHECK_REQUESTS_JOB_ID = "CheckForRequests";
    public const string CHECK_RESPONSES_JOB_ID = "CheckForResponses";

    public void ScheduleJobs<T>(string topic, string providerSessionId, string consumerSessionId, T? messageType) where T : notnull
    {
        if (typeof(T) != typeof(MessageTypes)) throw new ArgumentException($"Requires 'data' parameter to be of type {typeof(MessageTypes).FullName}");

        switch (messageType)
        {
            case MessageTypes.JSON:
                RecurringJob.AddOrUpdate<RequestJobJSON>(CHECK_REQUESTS_JOB_ID, x => x.CheckForRequests(providerSessionId, null!), Cron.Hourly);
                RecurringJob.AddOrUpdate<ResponseJobJSON>(CHECK_RESPONSES_JOB_ID, x => x.CheckForResponses(consumerSessionId, null!), Cron.Hourly);
                break;
            case MessageTypes.ExampleBOD:
                RecurringJob.AddOrUpdate<RequestJobBOD>(CHECK_REQUESTS_JOB_ID, x => x.CheckForRequests(providerSessionId, null!), Cron.Hourly);
                RecurringJob.AddOrUpdate<ResponseJobBOD>(CHECK_RESPONSES_JOB_ID, x => x.CheckForResponses(consumerSessionId, null!), Cron.Hourly);
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }
    }

    public void UnscheduleJobs()
    {
        RecurringJob.RemoveIfExists(CHECK_REQUESTS_JOB_ID);
        RecurringJob.RemoveIfExists(CHECK_RESPONSES_JOB_ID);
    }
}