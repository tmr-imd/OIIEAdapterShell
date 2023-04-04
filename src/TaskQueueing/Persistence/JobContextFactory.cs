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

        public Task<JobContext> CreateDbContext(ClaimsPrincipal claimsPrincipal)
        {
            return CreateDbContext( claimsPrincipal.Identity?.Name ?? "" );
        }

        public async Task<JobContext> CreateDbContext( string who )
        {
            var builder = new DbContextOptionsBuilder<JobContext>();
            var defaultConnection = _config.GetConnectionString("DefaultConnection");
            builder.UseSqlite($"Filename={defaultConnection}");
            var context = new JobContext(builder.Options, who);

            await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();

            return context;
        }
    }
}
