using Infrastructure.Data.DbContext.Configuration;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.DbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<FileModel> Files { get; set; } = null!;
        public DbSet<Folder> Folders { get; set; } = null!;
        public DbSet<DetectedObject> DetectedObjects { get; set; } = null;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Outbox> Outboxes { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DetectedObjectsEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FolderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FileEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new JobEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        }
    }
}
