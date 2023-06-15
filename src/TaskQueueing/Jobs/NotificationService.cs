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

    public static bool RecurringJobIsRunning(string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();

        string lastRunResult = string.Empty;
        var recurringJobs = connection.GetRecurringJobs(new[] { jobId });

        try
        {
            var jobState = connection.GetStateData(jobId);
            lastRunResult = jobState.Name; // For Example: "Succeeded", "Processing", "Deleted"

            return jobState.Name == "Processing";
        }
        catch
        {
            //job has not been run by the scheduler yet, swallow error
        }

        return false;
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
