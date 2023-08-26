using TaskQueueing.Jobs;
using AdapterServer.Shared;
using System.Xml.Linq;
using Hangfire;

namespace AdapterServer.Pages.Publication;

using ConsumerJobJSON = PubSubConsumerJob<ProcessNewStructuresJob, NewStructureAsset>;
using ConsumerJobBOD = PubSubConsumerJob<ProcessSyncStructureAssetsJob, XDocument>;
using ConfirmJob = PubSubConsumerJob<ProcessConfirmBODJob, string>;
using MessageTypes = PublicationViewModel.MessageTypes;

public class JobSchedulerForPubStructures : IScheduledJobsConfig<ManagePublicationViewModel>
{
    public const string POLL_STRUCTURES_JOB_ID = "PollNewStructureAssets";
    private const string CONFIRM_JOB_ID = "PollConfirmBOD";

    public void ScheduleJobs<T>(string topic, string providerSessionId, string consumerSessionId, T? data) where T : notnull
    {
        var (messageType, confirmationSessionId) = CheckConvertArgs(data);

        switch (messageType)
        {
            case MessageTypes.JSON:
                RecurringJob.AddOrUpdate<ConsumerJobJSON>(POLL_STRUCTURES_JOB_ID, x => x.PollSubscription(consumerSessionId, null!), Cron.Hourly);
                break;
            case MessageTypes.ExampleBOD:
                RecurringJob.AddOrUpdate<ConsumerJobBOD>(POLL_STRUCTURES_JOB_ID, x => x.PollSubscription(consumerSessionId, null!), Cron.Hourly);
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }

        RecurringJob.AddOrUpdate<ConfirmJob>(CONFIRM_JOB_ID, x => x.PollSubscription(confirmationSessionId, null!), Cron.Hourly);
    }

    private (MessageTypes, string) CheckConvertArgs<T>(T? data) where T : notnull
    {
        if (data is null) throw new ArgumentNullException("Requires non-null 'data' parameter.");
        if (data is not ValueTuple<MessageTypes, string>) throw new ArgumentException("The 'data' parameter must be a tuple (MessageTypes, string)");

        var convertedData = ((MessageTypes messageType, string ConfirmationSessionId))((object)data);
        
        if (string.IsNullOrWhiteSpace(convertedData.ConfirmationSessionId)) throw new ArgumentException("The data.ConfirmationSessionId parameter must not be empty");

        return convertedData;
    }

    public void UnscheduleJobs()
    {
        RecurringJob.RemoveIfExists(POLL_STRUCTURES_JOB_ID);
        RecurringJob.RemoveIfExists(CONFIRM_JOB_ID);
    }
}