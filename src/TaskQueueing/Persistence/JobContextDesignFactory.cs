using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Security.Claims;

namespace TaskQueueing.Persistence;

public class JobContextDesignFactory : IDesignTimeDbContextFactory<JobContext>
{
    //    public HollyContext CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<HollyContext>();

    //        return new HollyContext(optionsBuilder.Options);
    //    }
    //}

    private const string connectionString = "Server=.\\sqlexpress;Database=holly;Trusted_Connection=True;MultipleActiveResultSets=True";

    public JobContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<JobContext>();

        //optionsBuilder.UseSqlServer(connectionString, x => { x.MigrationsAssembly("TaskQueuing"); });

        //return new JobContext(optionsBuilder.Options, new ClaimsPrincipal());  //, null);
        return new JobContext(optionsBuilder.Options);
    }
}
