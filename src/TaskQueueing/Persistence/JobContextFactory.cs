using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace TaskQueueing.Persistence
{
    public class JobContextFactory
    {
        private readonly IConfiguration _config;
        public JobContextFactory(IConfiguration config)
        {
            _config = config;
        }

        public JobContext CreateDbContextNoDataKey(ClaimsPrincipal user)
        {
            var builder = new DbContextOptionsBuilder<JobContext>();
            var defaultConnection = _config.GetConnectionString("DefaultConnection");
            //builder.UseSqlServer(defaultConnection, x => { x.MigrationsAssembly("Holly"); });
            builder.UseSqlite($"Filename={defaultConnection}");
            //var context = new JobContext(builder.Options, user);
            var context = new JobContext(builder.Options);

            // Do this outside of CreateDbContextWithNoDataKey
            // await context.SetDataKey(user);

            return context;
        }

        public async Task<JobContext> CreateDbContext( ClaimsPrincipal user )
        {
            var context = CreateDbContextNoDataKey(user);
            await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();

            return context;
        }
    }
}
