using Hangfire;
using System.Security.Claims;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class NotificationJob
{
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public NotificationJob(JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.factory = factory;
        this.principal = principal;
    }

    public async Task Notify(string sessionId, string messageId, string requestMessageId)
    {
        using var context = await factory.CreateDbContext(principal);

        var recurringJobId = await NotificationService.JobIdFromSession(sessionId, context);

        if (string.IsNullOrWhiteSpace(recurringJobId))
        {
            // Either the sessionId no longer exists, or there's something wrong with the recurring job/id.
            // Regardless, don't throw an exception, we want things to end cleanly (to avoid DOS)
            return;
        }

        var lastJobId = NotificationService.GetLastJobId(recurringJobId);

        if (lastJobId is not null)
        {
            // Keep things simple. If there is a lastJobId, then schedule FirstCheck to run afterwards. It doesn't
            // matter if the job is currently running
            BackgroundJob.ContinueJobWith<NotificationJob>(lastJobId, x => x.FirstCheck(sessionId, messageId, requestMessageId), JobContinuationOptions.OnAnyFinishedState);
        }
        else
        {
            // The recurring task has never been run, so run FirstCheck now
            await FirstCheck(sessionId, messageId, requestMessageId);
        }
    }

    public async Task FirstCheck(string sessionId, string messageId, string requestMessageId)
    {
        using var context = await factory.CreateDbContext(principal);

        // Stop processing if the request message has been abandoned
        if (!string.IsNullOrEmpty(requestMessageId))
        {
            var requestMessage = await NotificationService.GetMessage(requestMessageId, context);

            // TODO: Add check for expiry
            if (requestMessage is not null && requestMessage.Processed)
                return;
        }
        else
        {
            var message = await NotificationService.GetMessage(messageId, context);

            if (message is not null && message.Processed)
                return;
        }

        var recurringJobId = await NotificationService.JobIdFromSession(sessionId, context);

        if (string.IsNullOrWhiteSpace(recurringJobId))
            return;

        var jobId = RecurringJob.TriggerJob(recurringJobId);
        BackgroundJob.ContinueJobWith<NotificationJob>(jobId, x => x.SecondCheck(messageId, requestMessageId), JobContinuationOptions.OnAnyFinishedState);
    }

    public async Task SecondCheck(string messageId, string requestMessageId)
    {
        using var context = await factory.CreateDbContext(principal);

        // Stop processing if the request message has been abandoned/processed
        if (!string.IsNullOrEmpty(requestMessageId))
        {
            var requestMessage = await NotificationService.GetMessage(requestMessageId, context);

            if (requestMessage is null || requestMessage.Processed)
                return;
        }

        var message = await NotificationService.GetMessage(messageId, context);

        if (message is not null && !message.Processed)
        {
            // TODO: Report error! Method yet to be determined
        }
    }
}