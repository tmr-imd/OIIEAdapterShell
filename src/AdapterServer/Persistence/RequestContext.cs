using Microsoft.EntityFrameworkCore;

namespace Holly.Persistence
{
    public class JobContext : DbContext, IJobContext
    {
        public JobContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Got this from https://www.youtube.com/watch?v=aUl5QfswNU4
            optionsBuilder.UseExceptionProcessor();

            #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging(true);
            #endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating( modelBuilder );

            ModelSchemas( modelBuilder );
            ModelRelationships( modelBuilder );
            ModelColumnTypes( modelBuilder );
        }

        private void ModelSchemas(ModelBuilder modelBuilder)
        {
        }
        private static void ModelRelationships(ModelBuilder modelBuilder)
        {
        }
        private static void ModelColumnTypes(ModelBuilder modelBuilder)
        {
        }

        private void SetAuditFields()
        {
            // Code derived from: https://www.infoq.com/articles/repository-advanced/
            var added = ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
            var modified = ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);

            var who = "";
            if (User != null) 
            {
                who = User.GetUserName();
                if (string.IsNullOrEmpty(who) && User.Identity != null)
                {
                    who = User.Identity.Name;
                }                
            }
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

        public void SetState( object entity, EntityState state )
        {
            Entry(entity).State = state;
        }
        public void SetValues( object entity, object values )
        {
            Entry(entity).CurrentValues.SetValues( values );
        }
    }
}
