using TaskQueueing.Jobs;
using TaskQueueing.Data;
using AdapterServer.Data;
using System.Xml.Linq;
using Hangfire;

namespace AdapterServer.Pages.Request;

using RequestJobJSON = RequestProviderJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using ResponseJobJSON = RequestConsumerJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using RequestJobBOD = RequestProviderJob<ProcessGetShowStructuresJob, XDocument, XDocument>;
using ResponseJobBOD = RequestConsumerJob<ProcessGetShowStructuresJob, XDocument, XDocument>;
using MessageTypes = RequestViewModel.MessageTypes;

public class JobSchedulerForStructures : IScheduledJobsConfig
{
    public void ScheduleJobs(RequestViewModel.MessageTypes messageType, string topic, string providerSessionId, string consumerSessionId)
    {
        switch (messageType)
        {
            case MessageTypes.JSON:
                RecurringJob.AddOrUpdate<RequestJobJSON>("CheckForRequests", x => x.CheckForRequests(providerSessionId, null!), Cron.Hourly);
                RecurringJob.AddOrUpdate<ResponseJobJSON>("CheckForResponses", x => x.CheckForResponses(consumerSessionId, null!), Cron.Hourly);
                break;
            case MessageTypes.ExampleBOD:
                RecurringJob.AddOrUpdate<RequestJobBOD>("CheckForRequests", x => x.CheckForRequests(providerSessionId, null!), Cron.Hourly);
                RecurringJob.AddOrUpdate<ResponseJobBOD>("CheckForResponses", x => x.CheckForResponses(consumerSessionId, null!), Cron.Hourly);
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }
    }

    public void UnscheduleJobs()
    {
        RecurringJob.RemoveIfExists("CheckForRequests");
        RecurringJob.RemoveIfExists("CheckForResponses");
    }
}