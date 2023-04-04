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

public class RequestProviderJob<T> where T : notnull
{
    private readonly IProviderRequest provider;
    private readonly JobContextFactory factory;

    public RequestProviderJob(IProviderRequest provider, JobContextFactory factory)
    {
        this.provider = provider;
        this.factory = factory;
    }

    public async Task<string> CheckForRequests( string sessionId )
    {
        try
        {
            var requestMessage = await provider.ReadRequest(sessionId);

            if (requestMessage is not null)
            {
                var content = requestMessage.MessageContent.Deserialise<T>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                BackgroundJob.Enqueue<RequestProviderJob<T>>(x => x.PostResponse(sessionId, requestMessage.Id, content, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                await provider.RemoveRequest(sessionId);

                return requestMessage.Id;
            }

        }
        catch ( IsbmFault ex ) when ( ex.FaultType == IsbmFaultType.SessionFault )
        {
            // Do nothing
        }

        return "";
    }
    public async Task<string> PostResponse(string sessionId, string requestId, T content, PerformContext ctx)
    {
        if ( content is StructureAssetsFilter filter)
        {
            var structures = StructureAssetService.GetStructures(filter);

            var response = await provider.PostResponse(sessionId, requestId, new RequestStructures(structures));

            using var context = await factory.CreateDbContext( new ClaimsPrincipal() );

            var storedResponse = new Response()
            {
                JobId = ctx.BackgroundJob.Id,
                ResponseId = response.Id,
                RequestId = requestId,
                Content = response.MessageContent.Content
            };

            await context.Responses.AddAsync( storedResponse );

            await context.SaveChangesAsync();

            return response.Id;
        }

        return "";
    }
}
