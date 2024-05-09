using Hangfire;
using System.Security.Claims;
using TaskQueueing.ObjectModel.Models;
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

    public async Task Notify(string sessionId, string messageId, NotifyBody body)
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
            BackgroundJob.ContinueJobWith<NotificationJob>(lastJobId, x => x.FirstCheck(sessionId, messageId, body), JobContinuationOptions.OnAnyFinishedState);
        }
        else
        {
            // The recurring task has never been run, so run FirstCheck now
            await FirstCheck(sessionId, messageId, body);
        }
    }

    public async Task FirstCheck(string sessionId, string messageId, NotifyBody body)
    {
        using var context = await factory.CreateDbContext(principal);

        // Must be a Response, stop processing if the corresponding Request has been abandoned
        if (!string.IsNullOrEmpty(body.requestMessageId))
        {
            var requestMessage = await NotificationService.GetMessage(body.requestMessageId, context);

            // TODO: Add check for expiry
            if (requestMessage is null || requestMessage.Processed)
                return;
        }

        var message = await NotificationService.GetMessage(messageId, context);

        if (message is not null && message.Processed)
            return;

        var recurringJobId = await NotificationService.JobIdFromSession(sessionId, context);

        if (string.IsNullOrWhiteSpace(recurringJobId)) return;

        var jobId = RecurringJob.TriggerJob(recurringJobId);
        if (string.IsNullOrWhiteSpace(jobId))
        {
            // must have already triggered/ended due to autoprocessing; run second check immediately to confirm or raise error
            await SecondCheck(messageId, body);
            return;
        }

        BackgroundJob.ContinueJobWith<NotificationJob>(jobId, x => x.SecondCheck(messageId, body), JobContinuationOptions.OnAnyFinishedState);
    }

    public async Task SecondCheck(string messageId, NotifyBody body)
    {
        using var context = await factory.CreateDbContext(principal);

        // Stop processing if the request message has been abandoned/processed
        if (!string.IsNullOrEmpty(body.requestMessageId))
        {
            var requestMessage = await NotificationService.GetMessage(body.requestMessageId, context);

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