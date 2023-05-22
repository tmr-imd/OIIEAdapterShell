using Notifications.ObjectModel;
using Notifications.ObjectModel.Models;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;
using AdapterQueue.Persistence.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Persistence;

public partial class NotificationsContext : ModelObjectContext, INotificationsContext
{
    public virtual DbSet<Notification> Notifications { get; set; } = null!;
    public virtual DbSet<NotificationState> NotificationStates { get; set; } = null!;

    public NotificationsContext(DbContextOptions<NotificationsContext> options, string who)
        : base(options, who)
    {
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

        modelBuilder.Entity<NotificationState>(entity =>
        {
            entity.Navigation<Notification>(x => x.Notification).AutoInclude();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
