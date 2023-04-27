using System.Security.Claims;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using PubMessage = TaskQueueing.ObjectModel.Models.Publication;

namespace AdapterServer.Pages.Publication;

public class ProcessNewStructuresJob : ProcessPublicationJob<NewStructureAsset>
{
    public ProcessNewStructuresJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override Task<bool> process(NewStructureAsset content, PubMessage publication, IJobContext context, ValidationDelegate<TaskQueueing.ObjectModel.Models.Publication> errorCallback)
    {
        // TODO
        return Task.FromResult(true);
    }

    protected override async Task<bool> validate(NewStructureAsset content, PubMessage publication, IJobContext context, ValidationDelegate<TaskQueueing.ObjectModel.Models.Publication> errorCallback)
    {
        bool success = true;

        if (String.IsNullOrWhiteSpace(content.Data.Code))
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, "New Structures must have a 'Code' field");
            onError(error, publication, context);
        }

        await Task.Yield();
        return success;
    }

    protected override void onError(MessageError error, PubMessage publication, IJobContext context)
    {
        base.onError(error, publication, context);
        Console.WriteLine("Encountered an error processing publication {0}: {1}", publication.MessageId, error);
    }
}