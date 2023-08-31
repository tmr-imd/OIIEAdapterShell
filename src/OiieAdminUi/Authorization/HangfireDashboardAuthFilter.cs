
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.AspNetCore;

namespace OiieAdminUi.Authorization;

public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        Console.WriteLine(context.GetHttpContext().Request.Headers);
#if DEBUG
        return true;
#endif
        throw new NotImplementedException("Need to connect this with the general authorization. E.g. ActiveDirectory/HTTP Headers from the AWS load balancer");
    }
}
