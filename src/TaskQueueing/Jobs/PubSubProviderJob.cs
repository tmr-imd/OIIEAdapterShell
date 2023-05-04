using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class PubSubProviderJob<T> where T : notnull
{
    private readonly IProviderPublication provider;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public PubSubProviderJob(IProviderPublication provider, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.provider = provider;
        this.factory = factory;
        this.principal = principal;
    }

    public async Task<string> PostPublication(string sessionId, T content, string topic, PerformContext ctx)
    {
        var publication = await provider.PostPublication(sessionId, content, topic);

        var storedPublication = new Publication
        {
            JobId = ctx.BackgroundJob.Id,
            State = MessageState.Posted | MessageState.Processed,
            MessageId = publication.Id,
            Topics = new[] { topic },
            MediaType = publication.MessageContent.MediaType,
            ContentEncoding = publication.MessageContent.ContentEncoding,
            Content = publication.MessageContent.Content
        };

        var context = await factory.CreateDbContext(principal);
        context.Publications.Add(storedPublication);
        await context.SaveChangesAsync();

        return publication.Id;
    }
}
