﻿using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestProviderJob
{
    private readonly IProviderRequest provider;
    private readonly JobContextFactory factory;

    public RequestProviderJob(IProviderRequest provider, JobContextFactory factory)
    {
        this.provider = provider;
        this.factory = factory;
    }

    public async Task<string> CheckForRequests()
    {
        using var context = await factory.CreateDbContext( new ClaimsPrincipal() );

        var sessionIds = await context.ChannelSettings.Select(x => x.SessionId).ToListAsync();

        foreach( var sessionId in sessionIds )
        {
            try
            {
                var requestMessage = await provider.ReadRequest(sessionId);

                if (requestMessage is not null)
                {
                    BackgroundJob.Enqueue<RequestProviderJob>(x => x.Respond(sessionId, requestMessage.Id, null));

                    await provider.RemoveRequest(sessionId);
                }

            }
            catch ( IsbmFault ex ) when ( ex.FaultType == IsbmFaultType.SessionFault )
            {
                // Do nothing
            }
        }

        return "";
    }
    public async Task<string> Respond(string sessionId, string requestId, PerformContext ctx)
    {
        await Task.Yield();

        return "";
    }
}
