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
    where TProcessJob : ProcessRequestResponseJob<TRequest, TResponse>
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
        var lastReadRequest = "";
        try
        {
            lastReadRequest = await readRemoveAll(sessionId);
        }
        catch ( IsbmFault ex ) when ( ex.FaultType == IsbmFaultType.SessionFault )
        {
            // Do nothing
        }

        return lastReadRequest;
    }

    public async Task<string> PostResponse(string sessionId, string requestId, TResponse content, PerformContext ctx)
    {
        var response = await provider.PostResponse(sessionId, requestId, content);

        using var context = await factory.CreateDbContext( principal );

        var request = await RequestProviderService.GetOpenRequest(requestId, context);
        if (request is null) return ""; // does not exist or already processed

        var storedResponse = new Response()
        {
            JobId = ctx.BackgroundJob.Id,
            State = MessageState.Posted,
            ResponseId = response.Id,
            RequestId = requestId,
            Request = request,
            MediaType = response.MessageContent.MediaType,
            ContentEncoding = response.MessageContent.ContentEncoding,
            Content = response.MessageContent.Content
        };

        context.Responses.Add( storedResponse );

        await context.SaveChangesAsync();

        return response.Id;
    }
    
    private async Task<string> readRemoveAll(string sessionId)
    {
        var context = await factory.CreateDbContext(principal);
        var lastReadRequest = "";
        
        for (var requestMessage = await provider.ReadRequest(sessionId); requestMessage is not null; requestMessage = await provider.ReadRequest(sessionId))
        {
            var content = requestMessage.MessageContent.Deserialise<TRequest>();

            var request = new Request
            {
                State = MessageState.Received,
                RequestId = requestMessage.Id,
                MediaType = requestMessage.MessageContent.MediaType,
                ContentEncoding = requestMessage.MessageContent.ContentEncoding,
                Content = requestMessage.MessageContent.Content
            };
            
            context.Requests.Add(request);
            await context.SaveChangesAsync();

            var jobId = BackgroundJob.Enqueue<TProcessJob>(x => x.ProcessRequest(sessionId, requestMessage.Id, content, null!));
            request.JobId = jobId;
            await context.SaveChangesAsync();

            await provider.RemoveRequest(sessionId);

            lastReadRequest = requestMessage.Id;
        }

        return lastReadRequest;
    }
}
