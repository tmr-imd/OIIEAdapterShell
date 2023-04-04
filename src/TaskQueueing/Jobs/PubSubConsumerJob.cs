using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class PubSubConsumerJob<T> where T : notnull
{
    private readonly IConsumerPublication consumer;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public PubSubConsumerJob(IConsumerPublication consumer, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.consumer = consumer;
        this.factory = factory;
        this.principal = principal;
    }

    public async Task<string> PollSubscription(string sessionId, PerformContext ctx)
    {
        try
        {
            var publication = await consumer.ReadPublication(sessionId);

            if (publication is not null)
            {
                using var context = await factory.CreateDbContext(principal);

                var storedPublication = new Publication()
                {
                    JobId = ctx.BackgroundJob.Id,
                    Content = publication.MessageContent.Content
                };

                await context.Publications.AddAsync(storedPublication);

                await context.SaveChangesAsync();

                await consumer.RemovePublication(sessionId);

                return publication.Id;
            }
        }
        catch (IsbmFault)
        {
            // Do nothing
        }

        return "";
    }
}
