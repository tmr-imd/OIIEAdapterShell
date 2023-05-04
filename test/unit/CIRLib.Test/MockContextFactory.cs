using CIRLib.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CIRLib.Test;

public class MockContextFactory
{
    [Fact]    
    public CIRLibContext getDbContext()
    {
        var task = new CIRLibContextFactory().CreateDbContext("unitTest");
        task.Wait();
        var context = task.Result;
        Assert.Empty(context.Registry.ToList());
        return context;
    }

}