using TaskQueueing.ObjectModel.Models;
using TaskQueueing.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;
using ModelBase.Persistence;
using ModelBase.Persistence.Configuration;
using AdapterQueue.Persistence.Configuration;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Text.Json;
using System.Reflection;

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

            modelBuilder.AddContainsFunctionTranslation();
        }
    }

    public static class SqlFunctionSupport
    {
        internal static void AddContainsFunctionTranslation(this ModelBuilder builder)
        {
            foreach (var method in typeof(SqlFunctionSupport).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                builder.HasDbFunction(method)
                    .HasTranslation(args => new SqlFunctionExpression(
                    "instr", // TODO: handle different functions for different DBMSs
                    new SqlExpression[] {
                        args[0],
                        args[1]
                    },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    type: typeof(bool),
                    typeMapping: args[1].TypeMapping
                    )
                )
                .HasParameter("_self").HasStoreType("none");
            }
        }

        public static bool Contains(this JsonDocument _self, string _other)
        {
            // Do not actually do anything, this is to trick LINQ for DB queries
            throw new NotSupportedException();
        }

        public static bool Contains(this IEnumerable<string> _self, string _other)
        {
            // This delegates to the normal extension method as this definition will hide it.
            // (Generic Methods cannot have HasDbFunction defined for them.)
            // The alternative is to not use HasDbFunction, and remember to always double cast
            // the property, e.g., `((string)(object)p.Topics).Contains("")`, so that the sQL
            // can be generated.
            return Enumerable.Contains(_self, _other);
        }
    }
}