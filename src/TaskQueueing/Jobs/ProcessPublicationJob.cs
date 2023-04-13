using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.ObjectModel;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public abstract class ProcessPublicationJob<TContent> where TContent : notnull
{
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public ProcessPublicationJob(JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.factory = factory;
        this.principal = principal;
    }

    public async Task ProcessPublication(string publicationId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        var publication = await PubSubConsumerService.GetUnprocessedPublication(publicationId, context);
        if (publication is null || publication.Content is null) return; // does not exist or already processed

        // We need to preserve the full MessageContent
        TContent content = new MessageContent(publication.Content, "").Deserialise<TContent>();

        if (!await validate(content, context))
        {
            // publication.Failed = true;
            // await context.SaveChangesAsync();
            await onValidationFailure(context);
            return;
        }

        publication.Processed = await process(content, context);
        await context.SaveChangesAsync();
    }

    protected abstract Task<bool> validate(TContent content, IJobContext context);
    protected abstract Task onValidationFailure(IJobContext context);
    protected abstract Task<bool> process(TContent content, IJobContext context);
}