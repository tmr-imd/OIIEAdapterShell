using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using TaskQueueing.Data;
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

    public async Task<string> PostRequest<T>(string sessionId, T value, string topic, PerformContext ctx) where T : notnull
    {
        var request = await consumer.PostRequest(sessionId, value, topic);

        var storedRequest = new Request()
        {
            JobId = ctx.BackgroundJob.Id,
            SessionId = sessionId,
            RequestId = request.Id,
            Filter = JsonSerializer.Serialize( value )
        };

        using var context = await factory.CreateDbContext(new System.Security.Claims.ClaimsPrincipal());

        context.Requests.Add(storedRequest);

        await context.SaveChangesAsync();

        return request.Id;
    }

    public async Task<string> CheckForResponses()
    {
        using var context = await factory.CreateDbContext(new ClaimsPrincipal());

        var openRequests = await context.Requests.Where( x => !x.Processed ).ToListAsync();

        foreach (var request in openRequests)
        {
            try
            {
                var requestMessage = await consumer.ReadResponse(request.SessionId, request.RequestId);

                if (requestMessage is not null)
                {
                    request.Content = JsonSerializer.Serialize( requestMessage.MessageContent.Content );
                    request.Processed = true;

                    await context.SaveChangesAsync();
                    await consumer.RemoveResponse(request.SessionId, request.RequestId);
                }
            }
            catch (IsbmFault ex)
            {
                // Do nothing
            }
            catch ( Exception ex)
            {
                Console.WriteLine( ex.Message ); 
            }
        }

        return "";
    }
}
