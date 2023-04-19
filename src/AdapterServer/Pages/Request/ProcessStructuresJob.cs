using System.Security.Claims;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using RequestMessage = TaskQueueing.ObjectModel.Models.Request;
using ResponseMessage = TaskQueueing.ObjectModel.Models.Response;

namespace AdapterServer.Pages.Request;

public class ProcessStructuresJob : ProcessRequestResponseJob<StructureAssetsFilter, RequestStructures>
{
    public ProcessStructuresJob(JobContextFactory factory, ClaimsPrincipal principal) : base(factory, principal)
    {
    }

    protected override async Task<RequestStructures> process(StructureAssetsFilter filter, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        await Task.Yield();
        var structures = StructureAssetService.GetStructures(filter);
        return new RequestStructures(structures);
    }

    protected override async Task<bool> process(RequestStructures content, ResponseMessage response, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        await Task.Yield();
        // This example does not do any special processing of the response
        // Will maybe update the data file or store entity "mappings" once the entity mapping tables are in place.
        return true;
    }

    protected override async Task<bool> validate(StructureAssetsFilter content, RequestMessage request, IJobContext context, ValidationDelegate<RequestMessage> errorCallback)
    {
        if (!(content.FilterCode.Any() || content.FilterCondition.Any() || 
                content.FilterInspector.Any() || content.FilterLocation.Any() || 
                content.FilterOwner.Any() || content.FilterType.Any()))
        {
            var warning = new MessageError(ErrorSeverity.Warning, "No filters were provided, this may return a very large number of results.");
            errorCallback(warning, request, context);
        }

        await Task.Yield();
        return true;
    }

    protected override async Task<bool> validate(RequestStructures content, ResponseMessage response, IJobContext context, ValidationDelegate<ResponseMessage> errorCallback)
    {
        // For demo purposes, treat no results as an Error
        bool success = true;

        if (!content.StructureAssets.Any())
        {
            success = false;
            var error = new MessageError(ErrorSeverity.Error, "No Structures were found!");
            errorCallback(error, response, context);

            // Is there a better way to also fail the Request?
            response.Request.Failed = true;
            response.Request.MessageErrors = response.Request.MessageErrors?.Append(error) ?? new[] { error };
        }

        await Task.Yield();
        return success;
    }

    // Simple example of overriding the error handler. Preserves default behaviour and writes to console.
    protected override void onError(MessageError error, RequestMessage request, IJobContext context)
    {
        base.onError(error, request, context);
        Console.WriteLine("Encountered an error processing request {0}: {1}", request.RequestId, error);
    }

    // Simple example of overriding the error handler. Preserves default behaviour and writes to console.
    protected override void onError(MessageError error, ResponseMessage response, IJobContext context)
    {
        base.onError(error, response, context);
        Console.WriteLine("Encountered an error processing response {0}: {1}", response.ResponseId, error);
    }
}
