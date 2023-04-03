using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestProviderJob
{
    private readonly IProviderRequest provider;
    private readonly JobContextFactory factory;

    public RequestProviderJob(IProviderRequest provider, JobContextFactory factory)
    {
        this.provider = provider;
        this.factory = factory;
    }

    public async Task<string> CheckForRequests()
    {
        using var context = await factory.CreateDbContext( new ClaimsPrincipal() );

        var sessionIds = await RequestProviderService.GetSessionIds( context );

        foreach( var sessionId in sessionIds )
        {
            try
            {
                var requestMessage = await provider.ReadRequest(sessionId);

                if (requestMessage is not null)
                {
                    var requestFilter = requestMessage.MessageContent.Deserialise<StructureAssetsFilter>();

                    BackgroundJob.Enqueue<RequestProviderJob>(x => x.PostResponse(sessionId, requestMessage.Id, requestFilter, null));

                    await provider.RemoveRequest(sessionId);

                    return requestMessage.Id;
                }

            }
            catch ( IsbmFault ex ) when ( ex.FaultType == IsbmFaultType.SessionFault )
            {
                // Do nothing
            }
        }

        return "";
    }
    public async Task<string> PostResponse<T>(string sessionId, string requestId, T requestFilter, PerformContext ctx)
    {
        if ( requestFilter is StructureAssetsFilter filter)
        {
            var structures = StructureAssetService.GetStructures(filter);

            var response = await provider.PostResponse(sessionId, requestId, new RequestStructures(structures));

            using var context = await factory.CreateDbContext( new ClaimsPrincipal() );

            var storedResponse = new Response()
            {
                JobId = ctx.BackgroundJob.Id,
                ResponseId = response.Id,
                RequestId = requestId,
                Content = JsonSerializer.Serialize(response.MessageContent.Content)
            };

            await context.Responses.AddAsync( storedResponse );

            await context.SaveChangesAsync();

            return response.Id;
        }

        return "";
    }
}
