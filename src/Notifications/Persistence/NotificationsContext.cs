using Notifications.ObjectModel;
using Notifications.ObjectModel.Models;
using TaskQueueing.ObjectModel.Models;
using AdapterQueue.Persistence.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Persistence;

public partial class NotificationsContext : DbContext, INotificationsContext
{
    private readonly string who;

    public virtual DbSet<Notification> Notifications { get; set; } = null!;

    public NotificationsContext(DbContextOptions<NotificationsContext> options, string who)
        : base(options)
    {
        this.who = who;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Got this from https://www.youtube.com/watch?v=aUl5QfswNU4
        // optionsBuilder.UseExceptionProcessor();

        #if DEBUG
            optionsBuilder.EnableSensitiveDataLogging(true);
        #endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Data).HasConversion<JsonDocumentConverter>();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private void SetAuditFields()
    {
        // Code derived from: https://www.infoq.com/articles/repository-advanced/
        var added = ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
        var modified = ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);

        var who = string.IsNullOrEmpty(this.who) ? "unknown" : this.who;

        foreach (var item in added)
        {
            if (item.Entity is ModelObject addedEntity)
            {
                addedEntity.DateCreated = DateTime.UtcNow;
                addedEntity.CreatedBy = who ?? "";
                addedEntity.DateModified = DateTime.UtcNow;
                addedEntity.ModifiedBy = who ?? "";
            }
        }

        foreach (var item in modified)
        {
            if (item.Entity is ModelObject modifiedEntity)
            {
                modifiedEntity.DateModified = DateTime.UtcNow;
                modifiedEntity.ModifiedBy = who ?? "";
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditFields();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetAuditFields();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
