using TaskQueueing.ObjectModel.Models;
using TaskQueueing.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using AdapterQueue.Persistence.Configuration;

namespace TaskQueueing.Persistence
{
    public class JobContext : ModelObjectContext, IJobContext
    {
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;
        public DbSet<Response> Responses { get; set; } = null!;
        public DbSet<Publication> Publications { get; set; } = null!;

        public JobContext(DbContextOptions options, string who) : base(options, who)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AbstractMessage>()
                .UseTpcMappingStrategy() // Table per class as before
                .Property(x => x.MessageErrors)
                .HasConversion<MessageErrorsConverter>();

            modelBuilder.Entity<AbstractMessage>()
                .Property(x => x.Content)
                .HasConversion<JsonDocumentConverter>();

            modelBuilder.Entity<Publication>()
                .Property(x => x.Topics)
                .HasConversion<TopicsConverter>();
        }
    }
}
