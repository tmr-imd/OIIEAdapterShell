using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;
using Notifications;
using Notifications.ObjectModel;

namespace TaskQueueing.Jobs;

public abstract class ProcessPublicationJob<TContent> where TContent : notnull
{
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;
    private readonly INotificationService notifications;

    public ProcessPublicationJob(JobContextFactory factory, ClaimsPrincipal principal, INotificationService notifications)
    {
        this.factory = factory;
        this.principal = principal;
        this.notifications = notifications;
    }

    public async Task ProcessPublication(string publicationId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        var publication = await PubSubConsumerService.GetUnprocessedPublication(publicationId, context);
        if (publication is null || publication.Content is null) return; // does not exist or already processed

        TContent content = new MessageContent(publication.Content, publication.MediaType, publication.ContentEncoding).Deserialise<TContent>();

        if (!await validate(content, publication, context, onError))
        {
            publication.Failed = true;
            await context.SaveChangesAsync();
            notifyListeners();
            return;
        }

        publication.Processed = await process(content, publication, context, onError);
        publication.Failed = !publication.Processed;
        await context.SaveChangesAsync();
        notifyListeners();
    }

    /// <summary>
    /// Default validation handler adds the error to the publication.
    /// </summary>
    /// <remarks>
    /// May be overridden by subclasses.
    /// Default behaviour appends the error to the message's errors list.
    /// </remarks>
    /// <param name="error">The error that was encountered</param>
    /// <param name="publication">The publication  being validated/processed</param>
    /// <param name="context">The database context</param>
    protected virtual void onError(MessageError error, Publication publication, IJobContext context)
    {
        publication.MessageErrors = publication.MessageErrors?.Append(error) ?? new[] { error };
    }

    protected virtual void notifyListeners()
    {
        _ = notifications.Notify(Scope.Internal, "publication-message-update", "Publication processed", "ProcessPublicationJob");
    }

    protected abstract Task<bool> validate(TContent content, Publication publication, IJobContext context, ValidationDelegate<Publication> errorCallback);
    protected abstract Task<bool> process(TContent content, Publication publication, IJobContext context, ValidationDelegate<Publication> errorCallback);
}