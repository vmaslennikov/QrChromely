using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChromelyAngular.Backend.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChromelyAngular.Backend.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<FileRequest> FileRequests { get; set; }
        public DbSet<PcrDocument> Documents { get; set; }
        public DbSet<PersonRequest> PersonRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var source = Path.Combine(Environment.CurrentDirectory, "qr.db");
            optionsBuilder.UseSqlite($"DataSource={source};");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            base.OnModelCreating(modelBuilder);
        }

        private static readonly EntityState[] states = new EntityState[] {
            EntityState.Added,
            EntityState.Modified,
            EntityState.Deleted
        };

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        private void OnBeforeSaving()
        {
            ChangeTracker.DetectChanges();
            // get entries that are being Added or Updated or Deleted
            var modifiedEntries = ChangeTracker
                .Entries()
                .Where(x => x.Entity is IAuditableObject && states.Contains(x.State));

            var now = DateTime.UtcNow;
            foreach (var entry in modifiedEntries)
            {
                if (entry.Entity is IAuditableObject auditableEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            auditableEntity.Modified = now;
                            break;
                        case EntityState.Added:
                            auditableEntity.Created = now;
                            auditableEntity.Modified = now;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}
