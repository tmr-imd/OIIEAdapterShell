using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class NotificationService
{
    public static async Task<string?> JobIdFromSession(string sessionId, JobContext context)
    {
        return await context.Sessions
            .Where(x => x.SessionId == sessionId)
            .Select(x => x.RecurringJobId)
            .FirstOrDefaultAsync();
    }

    public static string? GetLastJobId(string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();

        string lastRunResult = string.Empty;
        var recurringJobs = connection.GetRecurringJobs(new[] { jobId });

        return recurringJobs.Select(x => x.LastJobId).FirstOrDefault();
    }

    public static async Task<AbstractMessage?> GetMessage(string messageId, JobContext context)
    {
        var request = await context.Requests.Where(x => x.RequestId == messageId).FirstOrDefaultAsync();

        if (request is not null)
            return request;

        var response = await context.Responses.Where(x => x.ResponseId == messageId).FirstOrDefaultAsync();

        if (response is not null)
            return response;

        var publication = await context.Publications.Where(x => x.MessageId == messageId).FirstOrDefaultAsync();

        return publication;
    }
}
