using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CIRLib.Persistence;

public class JobContextDesignFactory : IDesignTimeDbContextFactory<CIRLibContext>
{
    public CIRLibContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CIRLibContext>();

        return new CIRLibContext(optionsBuilder.Options, "migrations");
    }
}
