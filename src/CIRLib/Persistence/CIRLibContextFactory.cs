using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CIRLib.Persistence
{
    public class CIRLibContextFactory
    {
        private readonly IConfiguration? _config;
        public CIRLibContextFactory(IConfiguration? config = null)
        {
            _config = config;
        }

        public Task<CIRLibContext> CreateDbContext(ClaimsPrincipal claimsPrincipal)
        {
           return CreateDbContext(claimsPrincipal.Identity?.Name ?? "");
        }

        public async Task<CIRLibContext> CreateDbContext( string who )
        {
            var builder = new DbContextOptionsBuilder<CIRLibContext>();
            var defaultConnection = _config?.GetConnectionString("CIRLibConnection") ?? "CIRLib.db";
            builder.UseSqlite($"Filename={defaultConnection}");
            var context = new CIRLibContext(builder.Options, who);
            await context.Database.MigrateAsync();
            
            return context;
        }
    }
}
