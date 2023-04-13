using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using System.Text.Json;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestConsumerJob<TProcessJob, TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
    where TProcessJob : ProcessMessageJob<TRequest, TResponse>
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
            RequestId = request.Id,
            Filter = JsonSerializer.SerializeToDocument(content)
        };

        using var context = await factory.CreateDbContext(principal);

        context.Requests.Add(storedRequest);

        await context.SaveChangesAsync();

        return request.Id;
    }

    public async Task<string> CheckForResponses( string sessionId )
    {
        using var context = await factory.CreateDbContext(principal);

        var openRequests = await RequestConsumerService.OpenRequests(context);

        if (!openRequests.Any())
            return "";

        var openRequest = openRequests.First();

        try
        {
            var responseMessage = await consumer.ReadResponse(sessionId, openRequest.RequestId);

            if (responseMessage is not null)
            {
                openRequest.Content = responseMessage.MessageContent.Content;
                await context.SaveChangesAsync();

                BackgroundJob.Enqueue<TProcessJob>(x => x.ProcessResponse(openRequest.RequestId, responseMessage.Id, null!));
                await consumer.RemoveResponse(sessionId, openRequest.RequestId);

                return responseMessage.Id;
            }
        }
        catch (IsbmFault)
        {
            // Do nothing
        }

        return "";
    }
}
