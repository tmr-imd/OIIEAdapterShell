using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskQueueing.ObjectModel.Enums;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestConsumerJob<TProcessJob, TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
    where TProcessJob : ProcessRequestResponseJob<TRequest, TResponse>
{
    private readonly IConsumerRequest consumer;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public RequestConsumerJob(IConsumerRequest consumer, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.consumer = consumer;
        this.factory = factory;
        this.principal = principal;
    }

    public async Task<string> PostRequest<T>(string sessionId, T content, string topic, PerformContext ctx) where T : notnull
    {
        var request = await consumer.PostRequest(sessionId, content, topic);

        var storedRequest = new Request()
        {
            JobId = ctx.BackgroundJob.Id,
            State = MessageState.Posted,
            RequestId = request.Id,
            Topic = topic,
            MediaType = request.MessageContent.MediaType,
            ContentEncoding = request.MessageContent.ContentEncoding,
            Content = request.MessageContent.Content
        };

        using var context = await factory.CreateDbContext(principal);

        context.Requests.Add(storedRequest);

        await context.SaveChangesAsync();

        return request.Id;
    }

    #if DEBUG
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    #endif
    public async Task<string> CheckForResponses( string sessionId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        var openRequests = await RequestConsumerService.OpenRequests(context);
        var lastResponseRead = "";

        foreach (var openRequest in openRequests)
        {
            try
            {
                lastResponseRead = await readRemoveAll(sessionId, openRequest, context, ctx);
            }
            catch (IsbmFault)
            {
                // TODO: appropriate error handling, e.g., may need to close the request
                // Some needs to be inside this loop, others outside the loop, such as session failure.
            }
        }

        return lastResponseRead;
    }

    private async Task<string> readRemoveAll(string sessionId, Request openRequest, JobContext context, PerformContext ctx)
    {
        var lastResponseRead = "";

        for (var responseMessage = await consumer.ReadResponse(sessionId, openRequest.RequestId); 
            responseMessage is not null; 
            responseMessage = await consumer.ReadResponse(sessionId, openRequest.RequestId))
        {
            var exists = await context.Responses.AnyAsync(x => x.ResponseId.ToLower() == responseMessage.Id.ToLower());
            if ( exists ) continue;

            var response = new Response
            {
                JobId = ctx.BackgroundJob.Id,
                State = MessageState.Received,
                ResponseId = responseMessage.Id,
                RequestId = openRequest.RequestId,
                Request = openRequest,
                MediaType = responseMessage.MessageContent.MediaType,
                ContentEncoding = responseMessage.MessageContent.ContentEncoding,
                Content = responseMessage.MessageContent.Content
            };

            context.Responses.Add(response);
            await context.SaveChangesAsync();

            BackgroundJob.Enqueue<TProcessJob>(x => x.ProcessResponse(openRequest.RequestId, responseMessage.Id, null!));
            await consumer.RemoveResponse(sessionId, openRequest.RequestId);

            lastResponseRead = responseMessage.Id;
        }

        return lastResponseRead;
    }
}
