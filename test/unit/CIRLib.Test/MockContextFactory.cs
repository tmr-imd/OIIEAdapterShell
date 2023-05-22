using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CIRLib.Persistence;

namespace CIRLib.Test;

public class MockContextFactory
{
    [Fact]    
    public CIRLibContext GetDbContext()
    {
        var builder = new DbContextOptionsBuilder<CIRLibContext>();
        var defaultConnection = "CIRLib.db";
        builder.UseSqlite($"Filename={defaultConnection}");
        var context = new CIRLibContext(builder.Options, "tester");
        var task = context.Database.EnsureDeletedAsync();
        task.Wait();
        var secondTask = context.Database.MigrateAsync();
        secondTask.Wait();
        Assert.Empty(context.Registry.ToList());
        return context;
    }

}