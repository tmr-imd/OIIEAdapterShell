using Hangfire;
using Hangfire.Server;
using Isbm2Client.Model;
using System.Security.Claims;
using System.Text.Json;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;
using Notifications;
using Notifications.ObjectModel;

namespace TaskQueueing.Jobs;

public delegate void ValidationDelegate<T>(MessageError error, T message, IJobContext context) where T : AbstractMessage;

public abstract class ProcessRequestResponseJob<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;
    private readonly INotificationService notifications;

    public ProcessRequestResponseJob(JobContextFactory factory, ClaimsPrincipal principal, INotificationService notifications)
    {
        this.factory = factory;
        this.principal = principal;
        this.notifications = notifications;
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
            notifyListeners();
            return;
        }

        var response = await process(content, request, context, onError);
        BackgroundJob.Enqueue<RequestProviderJob<ProcessRequestResponseJob<TResponse, TResponse>, TResponse, TResponse>>(x => x.PostResponse(sessionId, requestId, response, null!));
        
        request.Processing = true;
        await context.SaveChangesAsync();
        notifyListeners();
    }

    public async Task ProcessResponse(string requestId, string responseId, PerformContext ctx)
    {
        using var context = await factory.CreateDbContext(principal);

        var response = await RequestConsumerService.GetOpenResponse(requestId, responseId, context);
        if (response is null || response.Content is null) return; // does not exist or nothing to process

        TResponse content;
        try
        {
            content = new MessageContent(response.Content, response.MediaType, response.ContentEncoding).Deserialise<TResponse>();
        }
        catch (JsonException)
        {
            onDeserializationFailure(response.Content, response, context, onError);
            await context.SaveChangesAsync();
            notifyListeners();
            return;
        }

        if (!await validate(content, response, context, onError))
        {
            response.Failed = true;
            await context.SaveChangesAsync();
            notifyListeners();
            return;
        }

        response.Processed = await process(content, response, context, onError);
        response.Request.Processed = response.Processed;
        response.Request.Failed = !response.Processed;
        await context.SaveChangesAsync();
        notifyListeners();
    }

    /// <summary>
    /// Default validation handler adds the error to the request message.
    /// </summary>
    /// <remarks>
    /// May be overridden by subclasses.
    /// Default behaviour appends the error to the message's errors list.
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
    /// Default behaviour appends the error to the message's errors list.
    /// </remarks>
    /// <param name="error">The error that was encountered</param>
    /// <param name="response">The response message being validated/processed</param>
    /// <param name="context">The database context</param>
    protected virtual void onError(MessageError error, Response response, IJobContext context)
    {
        response.MessageErrors = response.MessageErrors?.Append(error) ?? new[] { error };
    }

    protected virtual void onDeserializationFailure(JsonDocument content, Request request, IJobContext context, ValidationDelegate<Request> errorCallback)
    {
        errorCallback(new MessageError(ErrorSeverity.Error, $"Unable to deserialize the request content as {typeof(TRequest).Name}"), request, context);
    }

    protected virtual void onDeserializationFailure(JsonDocument content, Response response, IJobContext context, ValidationDelegate<Response> errorCallback)
    {
        errorCallback(new MessageError(ErrorSeverity.Error, $"Unable to deserialize the response content as {typeof(TResponse).Name}"), response, context);
    }

    protected virtual void notifyListeners()
    {
        _ = notifications.Notify(Scope.Internal, "request-message-update", "Request/response processed", "ProcessRequestResponseJob");
    }

    protected abstract Task<bool> validate(TRequest content, Request request, IJobContext context, ValidationDelegate<Request> errorCallback);
    protected abstract Task<bool> validate(TResponse content, Response response, IJobContext context, ValidationDelegate<Response> errorCallback);
    protected abstract Task<TResponse> process(TRequest content, Request request, IJobContext context, ValidationDelegate<Request> errorCallback);
    protected abstract Task<bool> process(TResponse content, Response response, IJobContext context, ValidationDelegate<Request> errorCallback);
}