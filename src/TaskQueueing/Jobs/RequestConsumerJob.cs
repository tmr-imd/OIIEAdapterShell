using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using System.Text.Json;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestConsumerJob
{
    private readonly IConsumerRequest consumer;
    private readonly JobContextFactory factory;

    public RequestConsumerJob(IConsumerRequest consumer, JobContextFactory factory)
    {
        this.consumer = consumer;
        this.factory = factory;
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

        using var context = await factory.CreateDbContext(new ClaimsPrincipal());

        context.Requests.Add(storedRequest);

        await context.SaveChangesAsync();

        return request.Id;
    }

    public async Task<string> CheckForResponses( string sessionId )
    {
        using var context = await factory.CreateDbContext(new ClaimsPrincipal());

        var openRequests = await RequestConsumerService.OpenRequests(context);

        if (!openRequests.Any())
            return "";

        var openRequest = openRequests.First();

        try
        {
            var requestMessage = await consumer.ReadResponse(sessionId, openRequest.RequestId);

            if (requestMessage is not null)
            {
                openRequest.Content = requestMessage.MessageContent.Content;
                openRequest.Processed = true;

                await context.SaveChangesAsync();
                await consumer.RemoveResponse(sessionId, openRequest.RequestId);

                return requestMessage.Id;
            }
        }
        catch (IsbmFault)
        {
            // Do nothing
        }

        return "";
    }
}
