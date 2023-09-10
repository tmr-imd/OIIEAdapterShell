using ModelBase.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace ModelBase.Persistence;

public abstract class ModelObjectContext : DbContext
{
    private readonly string who;

    protected ModelObjectContext(DbContextOptions options, string who) : base(options)
    {
        this.who = who;
    }

    private void SetAuditFields()
    {
        // Code derived from: https://www.infoq.com/articles/repository-advanced/
        var added = ChangeTracker.Entries<ModelObject>().Where(x => x.State == EntityState.Added);
        var modified = ChangeTracker.Entries<ModelObject>().Where(x => x.State == EntityState.Modified);

        var who = string.IsNullOrWhiteSpace(this.who) ? "unknown" : this.who;

        foreach (var item in added)
        {
            var addedEntity = item.Entity;
            addedEntity.DateCreated = DateTime.UtcNow;
            addedEntity.CreatedBy = who ?? "";
            addedEntity.DateModified = DateTime.UtcNow;
            addedEntity.ModifiedBy = who ?? "";
        }

        foreach (var item in modified)
        {
            var modifiedEntity = item.Entity;
            modifiedEntity.DateModified = DateTime.UtcNow;
            modifiedEntity.ModifiedBy = who ?? "";
        }
    }

    public override int SaveChanges( bool acceptAllChangesOnSuccess )
    {
        SetAuditFields();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync( bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default )
    {
        SetAuditFields();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}