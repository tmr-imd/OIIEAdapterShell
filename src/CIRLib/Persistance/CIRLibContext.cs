using CIRLib.ObjectModel.Models;
using CIRLib.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace CIRLib.Persistence
{
    public class CIRLibContext : DbContext, ICIRLibContext
    {
        private readonly string who;

        public DbSet<Registry> Registry { get; set; } = null!;
        public DbSet<Category> Category { get; set; } = null!;
        // Using "new" below to hide the inhertied member 'DbContext.Entry()'.
        public new DbSet<Entry> Entry { get; set; } = null!;
        public DbSet<Property> Property { get; set; } = null!;
        public DbSet<PropertyValue> PropertyValue { get; set; } = null!;

        public CIRLibContext(DbContextOptions options, string who) : base(options)
        {
            this.who = who;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging(true);
            #endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Registry>();
            modelBuilder.Entity<Category>();
            modelBuilder.Entity<Entry>();
            modelBuilder.Entity<Property>();
            modelBuilder.Entity<PropertyValue>();
        }

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
