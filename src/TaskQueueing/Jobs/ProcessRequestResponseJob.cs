using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using System.Security.Claims;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public delegate void ValidationDelegate<T>(MessageError error, T message, IJobContext context) where T : AbstractMessage;

public abstract class ProcessRequestResponseJob<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public ProcessRequestResponseJob(JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.factory = factory;
        this.principal = principal;
    }

    public async Task ProcessRequest(string sessionId, string requestId, TRequest content, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);
        var request = await RequestProviderService.GetOpenRequest(requestId, context);
        if (request is null) return; // does not exist or is already processed

        if (!await validate(content, request, context, onError))
        {
            request.Failed = true;
            await context.SaveChangesAsync();
            return;
        }

        var response = await process(content, request, context, onError);
        BackgroundJob.Enqueue<RequestProviderJob<ProcessRequestResponseJob<TResponse, TResponse>, TResponse, TResponse>>(x => x.PostResponse(sessionId, requestId, response, null!));
        
        request.Processed = true;
        await context.SaveChangesAsync();
    }

    public async Task ProcessResponse(string requestId, string responseId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        var response = await RequestConsumerService.GetOpenResponse(requestId, responseId, context);
        if (response is null || response.Content is null) return; // does not exist or nothing to process

        TResponse content = new MessageContent(response.Content, response.MediaType, response.ContentEncoding).Deserialise<TResponse>();

        if (!await validate(content, response, context, onError))
        {
            response.Failed = true;
            await context.SaveChangesAsync();
            return;
        }

        response.Processed = await process(content, response, context, onError);
        response.Request.Processed = response.Processed;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Default validation handler adds the error to the request message.
    /// </summary>
    /// <remarks>
    /// May be overridden by subclasses.
    /// </remarks>
    /// <param name="error">The error that was encountered</param>
    /// <param name="request">The request message being validated/processed</param>
    /// <param name="context">The database context</param>
    protected virtual void onError(MessageError error, Request request, IJobContext context)
    {
        request.MessageErrors = request.MessageErrors?.Append(error) ?? new[] { error };
    }

    /// <summary>
    /// Default validation handler adds the error to the response message.
    /// </summary>
    /// <remarks>
    /// May be overridden by subclasses.
    /// </remarks>
    /// <param name="error">The error that was encountered</param>
    /// <param name="response">The response message being validated/processed</param>
    /// <param name="context">The database context</param>
    protected virtual void onError(MessageError error, Response response, IJobContext context)
    {
        response.MessageErrors = response.MessageErrors?.Append(error) ?? new[] { error };
    }

    protected abstract Task<bool> validate(TRequest content, Request request, IJobContext context, ValidationDelegate<Request> errorCallback);
    protected abstract Task<bool> validate(TResponse content, Response response, IJobContext context, ValidationDelegate<Response> errorCallback);
    protected abstract Task<TResponse> process(TRequest content, Request request, IJobContext context, ValidationDelegate<Request> errorCallback);
    protected abstract Task<bool> process(TResponse content, Response response, IJobContext context, ValidationDelegate<Request> errorCallback);
}