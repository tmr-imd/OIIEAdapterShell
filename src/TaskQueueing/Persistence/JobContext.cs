using TaskQueueing.ObjectModel.Models;
using TaskQueueing.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace TaskQueueing.Persistence
{
    public class JobContext : DbContext, IJobContext
    {
        public DbSet<ChannelSetting> ChannelSettings { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;
        public DbSet<Response> Responses { get; set; } = null!;

        public JobContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Got this from https://www.youtube.com/watch?v=aUl5QfswNU4
            //optionsBuilder.UseExceptionProcessor();

            #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging(true);
            #endif
        }

        private void SetAuditFields()
        {
            // Code derived from: https://www.infoq.com/articles/repository-advanced/
            var added = ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
            var modified = ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);

            var who = "";
            //if (User != null) 
            //{
            //    who = User.GetUserName();
            //    if (string.IsNullOrEmpty(who) && User.Identity != null)
            //    {
            //        who = User.Identity.Name;
            //    }                
            //}
            who = string.IsNullOrEmpty(who) ? "unknown" : who;

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
}
