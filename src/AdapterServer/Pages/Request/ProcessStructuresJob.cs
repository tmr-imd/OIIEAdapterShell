using System.Security.Claims;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;

namespace AdapterServer.Pages.Request;

public class ProcessStructuresJob : ProcessMessageJob<StructureAssetsFilter, RequestStructures>
{
    public ProcessStructuresJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override Task onValidationFailure(IJobContext context)
    {
        return Console.Error.WriteLineAsync("StructuresAssetFilter was invalid");
    }

    protected override async Task<RequestStructures> process(StructureAssetsFilter filter, IJobContext context)
    {
        await Task.Yield();
        var structures = StructureAssetService.GetStructures(filter);
        return new RequestStructures(structures);
    }

    protected override async Task<bool> process(RequestStructures content, IJobContext context)
    {
        await Task.Yield();
        // This example does not do any special processing of the response
        return true;
    }

    protected override async Task<bool> validate(StructureAssetsFilter content, IJobContext context)
    {
        await Task.Yield();
        return true;
    }

    protected override async Task<bool> validate(RequestStructures content, IJobContext context)
    {
        await Task.Yield();
        return true;
    }
}