﻿using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages;

public class RequestService
{
    public static async Task<IEnumerable<TaskModels.Request>> ListRequests( IJobContext context )
    {
        return await context.Requests
            .OrderByDescending( x => x.DateCreated )
            .ToListAsync();
    }

    public static async Task<TaskModels.Request?> GetRequest(  IJobContext context, string requestId )
    {
        return await context.Requests
            .Where(x => x.RequestId == requestId)
            .FirstOrDefaultAsync();
    }
}
