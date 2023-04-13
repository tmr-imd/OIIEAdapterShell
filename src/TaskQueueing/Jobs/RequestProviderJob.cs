using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestProviderJob<TProcessJob, TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
    where TProcessJob : ProcessMessageJob<TRequest, TResponse>
{
    private readonly IProviderRequest provider;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public RequestProviderJob(IProviderRequest provider, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.provider = provider;
        this.factory = factory;
        this.principal = principal;
    }

    public async Task<string> CheckForRequests( string sessionId )
    {
        try
        {
            var requestMessage = await provider.ReadRequest(sessionId);

            if (requestMessage is not null)
            {
                var content = requestMessage.MessageContent.Deserialise<TRequest>();

                BackgroundJob.Enqueue<TProcessJob>(x => x.ProcessRequest(sessionId, requestMessage.Id, content, null!));

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
    public async Task<string> PostResponse(string sessionId, string requestId, TResponse content, PerformContext ctx)
    {
        var response = await provider.PostResponse(sessionId, requestId, content);

        using var context = await factory.CreateDbContext( principal );

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
}
