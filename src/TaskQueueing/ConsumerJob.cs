using Hangfire.Server;
using Isbm2Client.Interface;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing;

public class ConsumerJob
{
    private readonly IConsumerRequest consumer;
    private readonly JobContextFactory factory;

    public ConsumerJob( IConsumerRequest consumer, JobContextFactory factory )
    {
        this.consumer = consumer;
        this.factory = factory;
    }

    public async Task<string> PostRequest<T>( string sessionId, T value, string topic, PerformContext ctx ) where T : notnull
    {
        var request = await consumer.PostRequest( sessionId, value, topic );

        var store = new Request()
        {
            JobId = ctx.BackgroundJob.Id,
            RequestId = request.Id
        };

        using var context = await factory.CreateDbContext(new System.Security.Claims.ClaimsPrincipal());

        context.Requests.Add( store );

        await context.SaveChangesAsync();

        return request.Id;
    }
}
