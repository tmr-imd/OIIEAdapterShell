using Hangfire.Server;
using Isbm2Client.Interface;
using System.Security.Claims;
using TaskQueueing.ObjectModel.Enums;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;
using Notifications;
using Notifications.ObjectModel;

namespace TaskQueueing.Jobs;

public class PubSubProviderJob<T> where T : notnull
{
    private readonly IProviderPublication provider;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;
    private readonly INotificationService notifications;

    public PubSubProviderJob(IProviderPublication provider, JobContextFactory factory, ClaimsPrincipal principal, INotificationService notifications)
    {
        this.provider = provider;
        this.factory = factory;
        this.principal = principal;
        this.notifications = notifications;
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

        using var context = await factory.CreateDbContext(principal);
        context.Publications.Add(storedPublication);
        await context.SaveChangesAsync();

        _ = notifications.Notify(Scope.Internal, "publication-message-update", $"Posted publication: {publication.Id}", "PubSubProviderJob")
            .ContinueWith(t => Console.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        return publication.Id;
    }
}
