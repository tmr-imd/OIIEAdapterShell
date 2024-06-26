﻿using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskQueueing.ObjectModel.Enums;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class PubSubConsumerJob<TProcessJob, TContent>
    where TContent : notnull
    where TProcessJob : ProcessPublicationJob<TContent>
{
    private readonly IConsumerPublication consumer;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public PubSubConsumerJob(IConsumerPublication consumer, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.consumer = consumer;
        this.factory = factory;
        this.principal = principal;
    }

    #if DEBUG
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    #endif
    public async Task<string> PollSubscription(string sessionId, PerformContext ctx)
    {
        var lastReadMessage = "";
        try
        {
            lastReadMessage = await readRemoveAll(sessionId, ctx);
        }
        catch (IsbmFault)
        {
            // Do nothing
        }

        return lastReadMessage;
    }

    private async Task<string> readRemoveAll(string sessionId, PerformContext ctx)
    {
        var lastReadMessage = "";

        for (var publication = await consumer.ReadPublication(sessionId);
                    publication is not null;
                    publication = await consumer.ReadPublication(sessionId))
        {
            using var context = await factory.CreateDbContext(principal);

            var exists = await context.Publications
                .WhereReceived()
                .Where(x => (x.State & (MessageState.Processing | MessageState.Processed)) != MessageState.Undefined)
                .AnyAsync(x => x.MessageId == publication.Id);

            if (exists)
            {
                await consumer.RemovePublication(sessionId);
                lastReadMessage = publication.Id;
                continue;
            }

            var storedPublication = new Publication()
            {
                JobId = ctx.BackgroundJob.Id,
                State = MessageState.Received,
                MessageId = publication.Id,
                Topics = publication.Topics,
                MediaType = publication.MessageContent.MediaType,
                ContentEncoding = publication.MessageContent.ContentEncoding,
                Content = publication.MessageContent.Content
            };

            context.Publications.Add(storedPublication);
            await context.SaveChangesAsync();

            BackgroundJob.Enqueue<TProcessJob>(x => x.ProcessPublication(storedPublication.MessageId, null!));
            await consumer.RemovePublication(sessionId);

            lastReadMessage = publication.Id;
        }

        return lastReadMessage;
    }
}
