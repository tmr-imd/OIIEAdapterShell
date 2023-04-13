using System.Security.Claims;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;

namespace AdapterServer.Pages.Publication;

public class ProcessNewStructuresJob : ProcessPublicationJob<NewStructureAsset>
{
    public ProcessNewStructuresJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override Task onValidationFailure(IJobContext context)
    {
        return Console.Error.WriteLineAsync("NewStructuresAsset was invalid");
    }

    protected override async Task<bool> process(NewStructureAsset content, IJobContext context)
    {
        await Task.Yield();
        // TODO
        return true;
    }

    protected override async Task<bool> validate(NewStructureAsset content, IJobContext context)
    {
        await Task.Yield();
        return !String.IsNullOrWhiteSpace(content.Data.Code);
    }
}