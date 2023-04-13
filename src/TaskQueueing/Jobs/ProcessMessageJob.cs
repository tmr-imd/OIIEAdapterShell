using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.ObjectModel;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public abstract class ProcessMessageJob<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public ProcessMessageJob(JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.factory = factory;
        this.principal = principal;
    }

    public async Task ProcessRequest(string sessionId, string requestId, TRequest content, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        if (!await validate(content, context))
        {
            // TODO: we currently do not track received requests other than through the job itself
            // but we need to ensure we track failures correctly.
            await onValidationFailure(context);
            return;
        }

        var response = await process(content, context);
        BackgroundJob.Enqueue<RequestProviderJob<ProcessMessageJob<TResponse, TResponse>, TResponse, TResponse>>(x => x.PostResponse(sessionId, requestId, response, null!));
    }

    public async Task ProcessResponse(string requestId, string responseId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        var request = await RequestConsumerService.GetOpenRequest(requestId, context);
        if (request is null || request.Content is null) return; // does not exist or already processed

        // We need to preserve the full MessageContent
        TResponse content = new MessageContent(request.Content, "").Deserialise<TResponse>();

        if (!await validate(content, context))
        {
            // request.Failed = true;
            // await context.SaveChangesAsync();
            await onValidationFailure(context);
            return;            
        }

        request.Processed = await process(content, context);
        await context.SaveChangesAsync();
    }

    public async Task ProcessPublication(string publicationId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

    }

    protected abstract Task<bool> validate(TRequest content, IJobContext context);
    protected abstract Task<bool> validate(TResponse content, IJobContext context);
    protected abstract Task onValidationFailure(IJobContext context);
    protected abstract Task<TResponse> process(TRequest content, IJobContext context);
    protected abstract Task<bool> process(TResponse content, IJobContext context);
}